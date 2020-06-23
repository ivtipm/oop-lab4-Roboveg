﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataBase_test1
{
    public partial class MainForm : Form
    {
        public DataMethods data = new DataMethods();
        public string oldValue = "";
        public string filename = "";
        
        public string ProcTitle;
        public string ProcFamily;
        public string ProcCoresThreads;
        public string ProcArch;
        public string ProcSocket;
        public string ProcYear;
        public string ProcID;

        public MainForm()
        {
            InitializeComponent();
            
            dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            openFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            saveFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";

            InitializeTimers();

            //Всплывающие подсказки для кнопок
            ToolTip t = new ToolTip();
            t.SetToolTip(button_OpenFile, "Открыть файл (Ctrl+O)");
            t.SetToolTip(button_SaveFile, "Сохранить файл (Ctrl+S)");
            t.SetToolTip(button_Search, "Поиск данных");
            t.SetToolTip(button_AddRow, "Добавление новых данных");
            t.SetToolTip(button_Sort, "Сортировать ЦП по году выпуска");

        }

        private void InitializeTimers()
        {
            // Таймер для автосохранения данных в файл раз в 1 минуту
            timer1.Interval = 60000;
            timer1.Tick += new EventHandler(timer1_Tick);

            // Таймер для отображения надписи, свидетельствующей о сохранении файла
            // длительностью 3 сек
            timer2.Interval = 3000;
            timer2.Tick += new EventHandler(timer2_Tick);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            data.SaveToFile(filename);
            labelAutoSave.Visible = true;
            timer2.Enabled = true;
            timer2.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (labelAutoSave.Visible)
            {
                labelAutoSave.Visible = false;
            }
            else
            {
                labelSave.Visible = false;
            }
            timer2.Enabled = false;
            timer2.Stop();
        }

        //Создаёт порядковый ID
        public ushort generateID()
        {
            int id = data.ProcFile.Count + 1;
            return (ushort)id;


            
        }

        //Режим добавления строк
        private void button_AddRow_Click(object sender, EventArgs e)
        {
            try
            {
                panelAdd.Visible = true;
                panel_Search.Visible = false;
                TextBoxsClear();
                TextBoxsEnabled();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }        
        }

        //Запрет редактирования последней строки
        public void BanChangeColumn(int index)
        {
            dataGrid.Rows[index].Cells[0].ReadOnly = true;
        }

        //Добавить новую запись в базу
        private void button_Add_Click(object sender, EventArgs e)
        {
            try
            {
                ProcID = textBox_ID.Text = generateID().ToString();

                ProcTitle = textBox_Title.Text;
                ProcFamily = comboBox_Family.Text;
                ProcCoresThreads = textBox_CoresThreads.Text;
                ProcYear = textBox_Year.Text;
                ProcArch = textBox_Arch.Text;
                ProcSocket = textBox_Socket.Text;

                CPU_Row proc = new CPU_Row(
                    (ushort)Convert.ToInt32(textBox_ID.Text),
                    textBox_Title.Text,
                    comboBox_Family.Text,
                    textBox_CoresThreads.Text,
                    (ushort)Convert.ToInt32(textBox_Year.Text),
                    textBox_Arch.Text,
                    textBox_Socket.Text);

                data.AddRow(proc);
                int n = data.ProcFile.Count;

                CPU_Row proc1 = (CPU_Row)data.ProcFile[n - 1];
                dataGrid.Rows.Add
                    (proc1.ProcID,
                    textBox_Title.Text,
                    comboBox_Family.Text,
                    textBox_CoresThreads.Text,
                    textBox_Year.Text,
                    textBox_Arch.Text,
                    textBox_Socket.Text);

                BanChangeColumn(n - 1);

                TextBoxsClear();

                if ((!timer1.Enabled) && (filename != ""))
                {
                    timer1.Enabled = true;
                    timer1.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        //Режим поиска данных
        private void button_Search_Click(object sender, EventArgs e)
        {
            try
            {
                panel_Search.Visible = true;
                panelAdd.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        //Полностью очистить базу
        private void удалитьВсёToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (data.ProcFile.Count != 0)
            {
                DialogResult dialogResult = MessageBox.Show
                    ("Уверены," +
                    "что хотите удалить все элементы?", 
                    "Подтверждение", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.Yes)
                {
                    data.DeleteAllRows();
                    dataGrid.Rows.Clear();
                }
            }
        }

        //Удаление отдельных записей в базе
        private void dataGrid_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            //try
            //{
            //    int count = dataGrid.Rows.Count;
            //    foreach (DataGridViewRow row in dataGrid.SelectedRows)
            //    {
            //        int index = row.Index; // индекс выбранной строки
            //        data.DeleteRow(index);
            //        dataGrid.Rows.RemoveAt(index);
            //        if (index == count - 1) return;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Ошибка: {ex.Message}");
            //}
        }

        //Завершение редактирование ячейки
        private void dataGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int indRow = dataGrid.Rows[e.RowIndex].Index;
                int indColumn = dataGrid.Columns[e.ColumnIndex].Index;
                object value = dataGrid.Rows[indRow].Cells[indColumn].Value;

                // Если значение не было введено, то оставляем старое
                if (value is null)
                {
                    MessageBox.Show("Вы не ввели значение.");
                    dataGrid.Rows[indRow].Cells[indColumn].Value = oldValue;
                    return;
                }

                if (indColumn == 1)
                {
                    data.EditProcTitle((string)value, indRow);
                }

                else
                if (indColumn == 2)
                {
                    data.EditProcFamily((string)value, indRow);
                }

                else
                if (indColumn == 3)
                {
                    data.EditProcCoresThreads((string)value, indRow);
                }

                else
                if (indColumn == 4)
                {
                    try
                    {
                        data.EditProcYear((ushort)Convert.ToInt32(value), indRow);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show($"Ошибка: {exception.Message}");
                        dataGrid.Rows[indRow].Cells[indColumn].Value = oldValue;
                    }
                }

                else
                if (indColumn == 5)
                {
                    data.EditProcArch((string)value, indRow);
                }

                else
                if (indColumn == 6)
                {
                    data.EditProcSocket((string)value, indRow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        //Начало редактирования ячейки
        private void dataGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (dataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                oldValue = ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
        }

        //Сохранить базу в текствовый файл
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (filename == "")
                {
                    if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                        return;
                    filename = saveFileDialog1.FileName;
                    this.Text = filename + " - База данных процессоров";
                }
                // Если сохранение произошло первый раз, то запускаем 
                // таймер для автосохранения
                if (!timer1.Enabled)
                {
                    timer1.Enabled = true;
                    timer1.Start();
                }
                // При каждом сохранении будет появляться надпись "Сохранение..."
                labelSave.Visible = true;
                timer2.Enabled = true;
                timer2.Start();

                data.SaveToFile(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        //Считать базу из файла
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }

                filename = openFileDialog1.FileName;
                this.Text = filename + " - База данных процессоров";
                dataGrid.Rows.Clear();
                data.OpenFile(filename);
                WriteToDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        //"Создать новый файл"
        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((data.ProcFile.Count != 0) || (filename != ""))
            {
                DialogResult dialogResult = MessageBox.Show
                    ("Уверены," +
                    "что хотите создать новый файл?" + "\r\n" +
                    "Изменения в текущем не сохранятся!", 
                    "Подтверждение", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.Yes)
                {
                    this.Text = "База данных процессоров";
                    filename = "";
                    data.DeleteAllRows();
                    dataGrid.Rows.Clear();
                    timer1.Stop();
                    timer1.Enabled = false;
                    timer2.Stop();
                    timer2.Enabled = false;
                    TextBoxsClear();
                }
            }
        }

        //Найти данные в базе
        private void button_SearchOK_Click(object sender, EventArgs e)
        {
            if ((data.ProcFile.Count == 0) || (textBox_Search.Text == ""))
                return;

            dataGrid.ClearSelection();
            List<int> foundElements = data.SearchRows(textBox_Search.Text);

            if (foundElements[0] == -1)
            {
                MessageBox.Show("Ничего не удалось найти!");
                return;
            }

            dataGrid.CurrentCell = dataGrid[0, foundElements[0]];
            for (int i = 0; i < foundElements.Count; i++)
            {
                dataGrid.Rows[foundElements[i]].Selected = true;
            }
        }

        //Дублирование функции открытия файла
        private void button_OpenFile_Click(object sender, EventArgs e)
        {
            открытьToolStripMenuItem_Click(открытьToolStripMenuItem, null);
        }

        //Дублирование функции сохранения файла
        private void button_SaveFile_Click(object sender, EventArgs e)
        {
            сохранитьToolStripMenuItem_Click(сохранитьToolStripMenuItem, null);
        }

        //Записать базу в таблицу
        private void WriteToDataGrid()
        {
            try
            {
                for (int i = 0; i < data.ProcFile.Count; i++)
                {
                    CPU_Row proc = (CPU_Row)data.ProcFile[i];
                    dataGrid.Rows.Add(
                        proc.ProcID,
                        proc.ProcTitle,
                        proc.ProcFamily,
                        proc.ProcCoresThreads,
                        proc.ProcYear,
                        proc.ProcArch,
                        proc.ProcSocket);

                    BanChangeColumn(i);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        //Очистить все поля
        private void TextBoxsClear()
        {
            textBox_ID.Text = "";
            textBox_Title.Text = "";
            comboBox_Family.Text = "";
            textBox_CoresThreads.Text = "";
            textBox_Year.Text = "";
            textBox_Arch.Text = "";
            textBox_Socket.Text = "";
        }

        //Сортировать записи
        private void button_Sort_Click(object sender, EventArgs e)
        {
            if (data.ProcFile.Count != 0)
            {
                dataGrid.Rows.Clear();
                if (comboBoxChoiceSort.SelectedIndex == 0)
                {
                    data.Sort(SortDirection.Ascending);
                }
                else
                {
                    data.Sort(SortDirection.Descending);
                }
                WriteToDataGrid();
            }
        }

        //"Справка"
        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string Info = "База данных процессоров" + "\n\n" +
                "Powered by Visual Studio 2019, C#" + "\n\n" + 
                "2020 г.";
            MessageBox.Show(Info, "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //Нажали на ячейку/строку
        private void dataGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int indRow = dataGrid.Rows[e.RowIndex].Index;
                CPU_Row _proc = (CPU_Row)data.ProcFile[indRow];

                CellClick_TextBoxInfo(_proc);
            }
            catch
            {

            }
        }

        //И данные из строки поместились в поля на форме
        private void CellClick_TextBoxInfo(CPU_Row proc)
        {
            button_AddRow_Click(button_AddRow, null);

            textBox_ID.Text = proc.ProcID.ToString();
            textBox_Title.Text = proc.ProcTitle;
            comboBox_Family.Text = proc.ProcFamily;
            textBox_CoresThreads.Text = proc.ProcCoresThreads;
            textBox_Year.Text = proc.ProcYear.ToString();
            textBox_Arch.Text = proc.ProcArch;
            textBox_Socket.Text = proc.ProcSocket;

            TextBoxsDisabled();            
        }

        //"открыть доступ к редактированию полей"
        private void TextBoxsEnabled()
        {
            panelAdd.Enabled = true;
            /*
            textBox_ID.Enabled = true;
            textBox_Title.Enabled = true;
            comboBox_Family.Enabled = true;
            textBox_CoresThreads.Enabled = true;
            textBox_Year.Enabled = true;
            textBox_Arch.Enabled = true;
            textBox_Socket.Enabled = true;*/
        }

        //"закрыть доступ к редактированию полей"
        private void TextBoxsDisabled()
        {
            panelAdd.Enabled = false;
            /*
            textBox_ID.Enabled = false;
            textBox_Title.Enabled = false;
            comboBox_Family.Enabled = false;
            textBox_CoresThreads.Enabled = false;
            textBox_Year.Enabled = false;
            textBox_Arch.Enabled = false;
            textBox_Socket.Enabled = false;*/
        }

        //"дублирование функции добавления строк"
        private void добавлениеСтрокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button_AddRow_Click(button_AddRow, null);
        }

        //"дублирование функции поиска строк"
        private void поискСтрокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button_Search_Click(button_Search, null);
        }

        //При закрытии формы сказать про несохранённые данные
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((data.ProcFile.Count != 0) || (filename != ""))
            {
                DialogResult dialogResult = MessageBox.Show
                    ("Уверены," +
                    "что хотите выйти из программы?" + "\r\n" +
                    "Изменения ещё не сохранены!",
                    "Подтверждение", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.Yes)
                {
                    this.Text = "База данных процессоров";
                    filename = "";
                    data.DeleteAllRows();
                    dataGrid.Rows.Clear();
                    timer1.Stop();
                    timer1.Enabled = false;
                    timer2.Stop();
                    timer2.Enabled = false;                    
                }
                else
                {
                    e.Cancel = true;                    
                }
            }
        }

        private void dataGrid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                object ObjID = e.Row.Cells[0].Value;
                ushort ID = (ushort)ObjID;
                CPU_Row proc;

                for (int i = 0; i < data.ProcFile.Count; i++)
                {
                    proc = (CPU_Row)data.ProcFile[i];

                    if(proc.ProcID == ID)
                    {
                        data.DeleteRow(i);
                        break;
                    }
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            
        }
    }
}
