using System;
using System.Collections.Generic;
using System.Text;

namespace DtoEntityProject
{
    public class ApiResponse
    {
        public ApiResponse()
        {
            Errors = new List<string>();

            Roles = new List<string>();
        }
        public List<string> Errors { get; set; }
        public string Token { get; set; }

        public object Result { get; set; }

        public List<string> Roles { get; set; }
    }
}
