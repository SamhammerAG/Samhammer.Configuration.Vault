using System;
using System.IO;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Kubernetes;
using VaultSharp.V1.AuthMethods.Token;

namespace Samhammer.Configuration.Vault.Sag.Services
{
    public static class VaultAuthService
    {
        private const string KubernetesServiceAccountJwtFile = "/var/run/secrets/kubernetes.io/serviceaccount/token";
        
        private const string EnvironmentVariableNameVaultDisabled = "VaultDisabled";
        private const string EnvironmentVariableNameVaultUrl = "VaultUrl";
        private const string EnvironmentVariableNameKubernetesRole = "VaultKubernetesRole";
        
        public static string GetVaultUrl()
        {
            var vaultUrl = IsRunningInKubernetesCluster()
                ? Environment.GetEnvironmentVariable(EnvironmentVariableNameVaultUrl)
                : ProcessExecutionService.RunCliProcess("sagctl", "vault get url");

            if (string.IsNullOrEmpty(vaultUrl))
            {
                throw new Exception($"Vault url may not be empty");
            }

            return vaultUrl;
        }

        public static IAuthMethodInfo GetAuthMethodInfo()
        {
            if (IsRunningInKubernetesCluster())
            {
                return GetKubernetesAuthMethodInfo();
            }

            return GetLocalAuthMethodInfo();
        }

        public static bool IsVaultDisabled()
        {
            bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableNameVaultDisabled), out var isVaultDisabled);
            return isVaultDisabled;
        }

        private static IAuthMethodInfo GetKubernetesAuthMethodInfo()
        {
            var role = Environment.GetEnvironmentVariable(EnvironmentVariableNameKubernetesRole);

            if (string.IsNullOrEmpty(role))
            {
                throw new Exception($"The environment variable '{EnvironmentVariableNameKubernetesRole}' may not be empty");
            }

            var jwt = File.ReadAllText(KubernetesServiceAccountJwtFile);

            if (string.IsNullOrEmpty(jwt))
            {
                throw new Exception($"Service account token may not be empty");
            }

            return new KubernetesAuthMethodInfo(role, jwt);
        }

        private static bool IsRunningInKubernetesCluster()
        {
            return File.Exists(KubernetesServiceAccountJwtFile);
        }

        private static IAuthMethodInfo GetLocalAuthMethodInfo()
        {
            var token = ProcessExecutionService.RunCliProcess("sagctl", "vault get token");
            return new TokenAuthMethodInfo(token);
        }
    }
}
