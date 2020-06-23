using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace DataBase_test1
{
    public class DataMethods
    {
        // Массив из строк с данными о процессорах
        public ArrayList ProcFile { get; } = new ArrayList();

        // Добавить новую строку в файл
        public void AddRow(CPU_Row procRow)
        {
            ProcFile.Add(procRow);
        }

        // Удалить определённую строку из массива
        public void DeleteRow(int number)
        {
            ProcFile.RemoveAt(number);
        }

        // Удалить все строки из массива
        public void DeleteAllRows()
        {
            ProcFile.Clear();
        }

        // Изменить название процессора
        public void EditProcTitle(string title, int index)
        {
            CPU_Row proc = (CPU_Row)ProcFile[index];
            proc.ProcTitle = title;
        }

        // Изменить семейство процессора
        public void EditProcFamily(string family, int index)
        {
            CPU_Row proc = (CPU_Row)ProcFile[index];
            proc.ProcFamily = family;
        }

        // Изменить кол-во ядер и потоков
        public void EditProcCoresThreads(string coresthreads, int index)
        {
            CPU_Row proc = (CPU_Row)ProcFile[index];
            proc.ProcCoresThreads = coresthreads;
        }

        // Изменить архитектуру процессора
        public void EditProcArch(string arch, int index)
        {
            CPU_Row proc = (CPU_Row)ProcFile[index];
            proc.ProcArch = arch;
        }

        // Изменить сокет процессора
        public void EditProcSocket(string socket, int index)
        {
            CPU_Row proc = (CPU_Row)ProcFile[index];
            proc.ProcSocket = socket;
        }

        // Изменить год выпуска процессора
        public void EditProcYear(ushort year, int index)
        {
            CPU_Row proc = (CPU_Row)ProcFile[index];
            if ((year < 1971) || (year > (DateTime.Now.Year) + 10))
            {
                throw new Exception("Год выпуска должен быть не раньше 1971 и не позднее " + (DateTime.Now.Year) + 10);
            }
            proc.ProcYear = year;
        }

        // Сохранить массив в файл
        public void SaveToFile(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.Unicode))
            {
                foreach (CPU_Row s in ProcFile)
                {
                    sw.WriteLine(s.ToString());
                }
            }
        }

        // Загрузить массив из файла
        public void OpenFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new Exception("Файл не существует");
            }
                
            if (ProcFile.Count != 0)
            {
                DeleteAllRows();
            }
                
            using (StreamReader sw = new StreamReader(filename))
            {
                while (!sw.EndOfStream)
                {
                    string str = sw.ReadLine();
                    string[] dataFromFile = str.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                    ushort id = (ushort)Convert.ToInt32(dataFromFile[0]);
                    string title = dataFromFile[1];
                    string family = dataFromFile[2];
                    string coresthreads = dataFromFile[3];
                    ushort year = (ushort)Convert.ToInt32(dataFromFile[4]);
                    string arch = dataFromFile[5];
                    string socket = dataFromFile[6];

                    CPU_Row procRow = new CPU_Row(id, title, family, coresthreads, year, arch, socket);
                    AddRow(procRow);
                }
            }
        }

        // Поиск совпадений по базе, и получение ID всех найденных записей        
        // Возвращает -1, если совпадений не найдено
        public List<int> SearchRows(string query)
        {
            List<int> count = new List<int>();

            //Проверяются ID и год выпуска
            if (ushort.TryParse(query, out ushort num_query))
            {
                for (int i = 0; i < ProcFile.Count; i++)
                {
                    CPU_Row procRow = (CPU_Row)ProcFile[i];

                    if (procRow.ProcID == num_query)
                    {
                        count.Add(i);
                        break; // Если нашли запись по уникальному ID, то закончить поиск
                    }
                    else 
                    {
                        if (procRow.ProcYear == num_query)
                        {
                            count.Add(i);
                        }                            
                    }
                }

                if (count.Count == 0)
                {
                    count.Add(-1);
                }
                return count;
            }

            // Поиск по текстовым полям записи
            query = query.ToLower(); // перевод в нижний регистр
            query = query.Replace(" ", "");

            for (int i = 0; i < ProcFile.Count; i++)
            {
                CPU_Row procRow = (CPU_Row)ProcFile[i];

                if (procRow.ProcTitle.ToLower().Replace(" ", "").Contains(query))
                {
                    count.Add(i);
                }

                else
                if (procRow.ProcFamily.ToLower().Replace(" ", "").Contains(query))
                {
                    count.Add(i);
                }

                else
                if (procRow.ProcCoresThreads.ToLower().Replace(" ", "").Contains(query))
                {
                    count.Add(i);
                }

                else
                if (procRow.ProcArch.ToLower().Replace(" ", "").Contains(query))
                {
                    count.Add(i);
                }

                else
                if (procRow.ProcSocket.ToLower().Replace(" ", "").Contains(query))
                {
                    count.Add(i);
                }
            }

            if (count.Count == 0)
            {
                count.Add(-1);
            }
            return count;
        }

        // Сортировка по году выпуска процессора
        public void Sort(SortDirection direction)
        {
            ProcFile.Sort(new YearComparer(direction));
        }
    }

    // Сортировка по возрастанию/убыванию
    public enum SortDirection
    {
        Ascending, // возрастание
        Descending // убывание
    }

    public class YearComparer : IComparer
    {
        private SortDirection m_direction = SortDirection.Ascending;

        public YearComparer() : base() { }

        public YearComparer(SortDirection direction)
        {
            m_direction = direction;
        }

        int IComparer.Compare(object x, object y)
        {
            CPU_Row procRow1 = (CPU_Row)x;
            CPU_Row procRow2 = (CPU_Row)y;

            return (m_direction == SortDirection.Ascending) ? 
                procRow1.ProcYear.CompareTo(procRow2.ProcYear) :
                procRow2.ProcYear.CompareTo(procRow1.ProcYear);
        }
    }
}
