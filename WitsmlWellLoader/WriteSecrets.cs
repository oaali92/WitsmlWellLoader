using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace WitsmlWellLoader
{
    public class WriteSecrets
    {
        private readonly ServerCreds serverCreds;

        public WriteSecrets(IOptions<ServerCreds> serverCreds)
        {
            this.serverCreds = serverCreds.Value;
        }

        public void Write()
        {
            Console.WriteLine($"Value of url is '{serverCreds.url}'");
            Console.WriteLine($"Value of username is '{serverCreds.username}'");
            Console.WriteLine($"Value of pwd is '{serverCreds.password}'");

            Console.ReadLine();
        }
    }
}
