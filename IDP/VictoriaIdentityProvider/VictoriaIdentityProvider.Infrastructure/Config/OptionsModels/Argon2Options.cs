using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Infrastructure.Config.OptionsModels
{
    public class Argon2Options
    {
        public string Signature => $"$argon2id$v=19$m={MemorySize},t={Iterations},p={Paralelism}";
        public int MemorySize { get; set; }
        public int Iterations { get; set; }
        public int Paralelism { get; set; }
        public int SaltSize { get; set; }
        public int HashSize { get; set; }
        public string Pepper { get; set; } = string.Empty;
    }
}
