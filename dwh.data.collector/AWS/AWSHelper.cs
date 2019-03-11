using Amazon.Runtime.CredentialManagement;
using dwh.data.collector.Config;
using toolBox;


namespace dwh.data.collector.AWS
{
    static class AWSHelper
    {
        private static void GetCredentials(ref string[] s)
        {
            Crypto crypto = new Crypto();
            s[0] = crypto.Decrypt(AppConfig.GetString("acces_key","","AWS","AWS.json"));
            s[1] = crypto.Decrypt(AppConfig.GetString("secret_key","","AWS","AWS.json"));
        }

        public static CredentialProfile CreateAWSProfile(ref string[] credentials)
        {
            string[] s = new string[1];
            GetCredentials(ref s);
            CredentialProfileOptions cpo = new CredentialProfileOptions();
            cpo.AccessKey = s[0];
            cpo.SecretKey = s[1];
            CredentialProfile cp = new CredentialProfile("ff_runtime", cpo);
            return cp;
        }
    }
}
