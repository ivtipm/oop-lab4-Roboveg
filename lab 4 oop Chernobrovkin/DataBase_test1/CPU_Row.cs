using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase_test1
{
    public class CPU_Row
    {
        public CPU_Row(
            ushort procID, 
            string procTitle, 
            string procFamily, 
            string procCoresThreads, 
            ushort procYear, 
            string procArch, 
            string procSocket)
        {
            if ((procTitle == "") ||(procFamily == "") ||(procCoresThreads == "") ||(procArch == "") ||(procSocket == ""))
            {
                throw new Exception("Проверьте, всё ли вы заполнили. Все поля должны быть заполнены");                
            }
            ProcTitle = procTitle;
            ProcFamily = procFamily;
            ProcCoresThreads = procCoresThreads;
            ProcArch = procArch;
            ProcSocket = procSocket;

            if ((procYear < 1971) || (procYear > (DateTime.Now.Year + 10)))
            {
                throw new Exception("Год выпуска должен быть не раньше 1971 и не позднее 2030");
            }

            ProcYear = procYear;
            ProcID = procID;
        }

        // Свойства для полей
        public string ProcTitle     { get; set; } // | Название процессора
        public string ProcFamily     { get; set; } // | Семейство процессора
        public string ProcCoresThreads  { get; set; } // | Кол-во ядер и потоков процессора
        public string ProcArch   { get; set; } // | Архитектура процессора
        public string ProcSocket  { get; set; } // | Сокет процессора 
        public ushort ProcYear   { get; set; } // | Год выпуска процессора
        public ushort ProcID        { get; set; } // | ID записи в таблице
        
        public override string ToString()
        {
            return ProcID + " | " + ProcTitle + " | " + ProcFamily + " | " + ProcCoresThreads + " | " + ProcYear + " | " + ProcArch + " | " + ProcSocket + " | " ;
            //return ProcID + " | " + ProcTitle + " | " + ProcFamily + " | " + ProcCoresThreads + " | " + ProcArch + " | " + ProcSocket + " | " + ProcYear + " | ";
        }
    }
}
