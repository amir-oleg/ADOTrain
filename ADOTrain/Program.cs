using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOTrain
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args == null ||args.Length==0)
                return;
            var query = new Query();
            if (args[0] == "-init" && args.Length==1)
            {
                query.InitDb();
            }

            if (args[0] == "-lecture" && args.Length == 3)
            {
                if (DateTime.TryParse(args[1], out var date))
                {
                    Console.WriteLine("Затронуто строк " + query.AddLecture(date,args[2]));
                }
            }

            if (args[0] == "-student" && args.Length == 2)
            {
                Console.WriteLine("Затронуто строк " + query.AddStudent(args[1]));
            }

            if (args[0] == "-attend" && args.Length == 4)
            {
                if (DateTime.TryParse(args[2], out var date))
                {
                    if(int.TryParse(args[3], out var mark))
                        Console.WriteLine("Затронуто строк "+query.AddAttend(args[1], date, mark));
                }
            }

            if (args[0] == "-report" && args.Length == 1)
            {
                query.PrintReport();
            }

            Console.Read();
        }
    }
}
