using System;
using System.Collections.Generic;

namespace ADOTrain.Dtos
{
    public class AttendanceDto
    {
        public string Topic { get; set; }
        public DateTime LectureDate { get; set; }
        public List<string> Students { get; set; }
    }
}
