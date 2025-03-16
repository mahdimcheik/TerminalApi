using System.Xml.Linq;
using TerminalApi.Models.Role;

namespace TerminalApi.Utilities
{
    public static class HardCode
    {
        public static Role Student =>
            new Role()
            {
                Id = "7f56db63-4e78-44a8-b681-ec1490a9b29s",
                Name = "Student",
                NormalizedName = "STUDENT",
                ConcurrencyStamp = "7f56db63-4e78-44a8-b681-ec1490a9b29s",
            };
        public static Role Teacher =>
            new Role()
            {
                Id = "7f56db63-4e78-44a8-b681-ec1490a9b29T",
                Name = "Teacher",
                NormalizedName = "TEACHER",
                ConcurrencyStamp = "7f56db63-4e78-44a8-b681-ec1490a9b29d",
            };

        public static Role Admin =>
            new Role()
            {
                Id = "63a2a3ac-442e-4e4c-ad91-1443122b5a6a",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "63a2a3ac-442e-4e4c-ad91-1443122b5a6a",
            };
        public static Role Client =>
            new Role()
            {
                Id = "12ccaa16-0d50-491e-8157-ec1b133cf120",
                Name = "Client",
                NormalizedName = "CLIENT",
                ConcurrencyStamp = "12ccaa16-0d50-491e-8157-ec1b133cf120",
            };

        public static string TeacherId = "1577fcf3-35a3-42fb-add1-daffcc56f640";
    }
}
