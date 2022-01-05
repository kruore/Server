using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
namespace VoucherDB
{
    class GM_DBManager
    {
        public GM_DBManager()
        {
            init();
        }
        public static GM_DBManager inst;
        public MySqlConnection ConnectionDB()
        {
            MySqlConnection connection = new MySqlConnection("Server=localhost;Port=3306;Database=coding32;UiD=root;Pwd=Cogsci2017!;");
            return connection;
        }
        public void init()
        {
            if (inst != null)
            {
                inst = this;
            }
        }
        public bool CheckID(string _idx)
        {
            try
            {
                int scheck = CheckSchema(_idx);
                if (scheck == -1 || scheck == 0)
                {
                    Log(scheck.ToString());
                }
                return true;
            }
            catch
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 새로운 id값이 들어오면 새로운 데이터베이스(schema)를 생성
        /// </summary>
        /// <param name="_idx"></param>
        public void CreateDataBase(string _idx)
        {
            using (MySqlConnection connection = ConnectionDB())
            {
                string insertQuery = string.Format("Create schema {0};", _idx);
                Log(insertQuery);
                try
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(insertQuery, connection);
                    if (command.ExecuteNonQuery() == -1 || command.ExecuteNonQuery() == 0)
                    {
                        Log("데이터베이스 생성 실패");
                    }
                    else
                    {
                        Log("데이터베이스 생성 성공");
                    }
                    connection.Close();
                }
                catch (Exception e)
                {
                    Log("실패");
                    Log(e.ToString());
                    connection.Close();
                }
            }
        }
        public void InitSchema(string _idx)
        {
            CreateDataBase(_idx);
            CreateTable(_idx, "DataPath");
            Addcolumn(_idx, "DataPath", "filename", "varchar(30)");
            CreateTable(_idx, "tableDataSet");
            Addcolumn(_idx, "tableDataSet", "weight", "int");
            Addcolumn(_idx, "tableDataSet", "count", "int");
            Addcolumn(_idx, "tableDataSet", "time", "float");
            Addcolumn(_idx, "tableDataSet", "machineindex", "int");
            Addcolumn(_idx, "tableDataSet", "exerciseclass", "int");
            Addcolumn(_idx, "tableDataSet", "muscleclass", "int");
        }
        /// <summary>
        /// 테이블 생성
        /// </summary>
        /// <param name="_tablename">table이름</param>
        public void CreateTable(string _idx, string _tablename)
        {
            using (MySqlConnection connection = ConnectionDB())
            {
                string insertQuery = string.Format("use {0};Create Table {1}(DateData varchar(30));", _idx, _tablename);
                Log(insertQuery);
                try
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(insertQuery, connection);
                    int debugtext = command.ExecuteNonQuery();
                    Log(debugtext.ToString());
                    if (debugtext == 1)
                    {
                        Log("테이블 생성 성공");
                    }
                    else
                    {
                        Log("테이블 생성 실패");
                    }
                    connection.Close();
                }
                catch (Exception e)
                {
                    Log("실패");
                    Log(e.ToString());
                }
            }
        }
        public int CheckSchema(string _idx)
        {
            using (MySqlConnection connection = ConnectionDB())
            {
                string insertQuery = string.Format("SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = '{0}';", _idx);
                Log(insertQuery);
                try
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(insertQuery, connection);
                    MySqlDataReader rdr = command.ExecuteReader();
                    StringBuilder stringBuilder = new StringBuilder();
                    while (rdr.Read())
                    {
                        stringBuilder.Append(rdr.GetString(0));
                    }
                    rdr.Close();
                    connection.Close();
                    Log(stringBuilder.ToString());
                    int count;
                    if (int.TryParse(stringBuilder.ToString(), out count))
                    {
                        if (count <= 0)
                        {
                            InitSchema(_idx);
                        }
                    }
                    return count;
                }
                catch (Exception e)
                {
                    Log("실패");
                    Log(e.ToString());
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public MySqlCommand insertcommand(string _query, MySqlConnection _connection)
        {
            Log(_query);
            return new MySqlCommand(_query, _connection);
        }
        /// <summary>
        /// 테이블에 컬럼을 추가합니다.
        /// </summary>
        /// <param name="_tablename">추가할 테이블이름</param>
        /// <param name="_columnname">추가할 컬럼명</param>
        /// <param name="_datatype">추가할 컬럼의 데이터 타입</param>
        public void Addcolumn(string _idx, string _tablename, string _columnname, string _datatype)
        {
            using (MySqlConnection connection = ConnectionDB())
            {
                string insertQuery = string.Format("use {0};ALTER Table {1} add {2} {3};", _idx, _tablename, _columnname, _datatype);

                Log(insertQuery.ToString());
                try
                {

                    connection.Open();
                    MySqlCommand command = new MySqlCommand(insertQuery, connection);
                    if (command.ExecuteNonQuery() == -1)
                    {
                        Log("테이블 생성 실패");
                    }
                    else
                    {
                        Log("테이블 생성 성공");
                    }

                    connection.Close();
                }
                catch (Exception e)
                {
                    Log("실패");
                    Log(e.ToString());
                }
            }
        }
        public void Log(string _log)
        {
            Console.WriteLine(_log);
        }
        public bool CheckDataPathData(string _idx, string _dateData, string _filename)
        {
            using (MySqlConnection connection = ConnectionDB())
            {
                string insertQuery = string.Format("use {0}; SELECT COUNT(*) FROM DataPath WHERE filename= \"{1}\" and datedata = \"{2}\";", _idx, _filename, _dateData);
                Log(insertQuery);
                try
                {

                    connection.Open();
                    MySqlCommand command = new MySqlCommand(insertQuery, connection);
                    MySqlDataReader rdr = command.ExecuteReader();
                    StringBuilder stringBuilder = new StringBuilder();
                    while (rdr.Read())
                    {
                        stringBuilder.Append(rdr.GetString(0));
                    }
                    rdr.Close();
                    int count;

                    connection.Close();
                    if (int.TryParse(stringBuilder.ToString(), out count))
                    {
                        if (count > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log("실패");
                    Log(e.ToString());
                }
            }
            return false;
        }
        public void UpdateDataPath(string _idx, string _dateData, string _filename)
        {
            if (!CheckDataPathData(_idx, _filename, _dateData))
            {
                InsertDataPath(_idx, _dateData, _filename);
            }
            else
            {
                Log("이미 존재하는 데이터 입니다.");
            }
        }
        public void InsertDataPath(string _idx, string _dateData, string _filename)
        {
            using (MySqlConnection connection = ConnectionDB())
            {
                string insertQuery = string.Format("use {0};insert into datapath(Datedata,filename) value ({1},{2});", _idx, _dateData, _filename);
                Log(insertQuery.ToString());
                try
                {

                    connection.Open();
                    MySqlCommand command = new MySqlCommand(insertQuery, connection);
                    if (command.ExecuteNonQuery() == -1)
                    {
                        Log("테이블 생성 실패");
                    }
                    else
                    {
                        Log("테이블 생성 성공");
                    }

                    connection.Close();
                }
                catch (Exception e)
                {
                    Log("실패");
                    Log(e.ToString());
                }
            }
        }
        public bool CheckDataset(string _idx, string _datedata, int _weight, int _machineindex)
        {
            using (MySqlConnection connection = ConnectionDB())
            {
                string insertQuery = string.Format("use {0}; SELECT Count(*) FROM tableDataset WHERE datedata= \"{1}\" and weight = {2} and machineindex = {3};", _idx, _datedata, _weight, _machineindex);
                Log(insertQuery);
                try
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(insertQuery, connection);
                    MySqlDataReader rdr = command.ExecuteReader();
                    StringBuilder stringBuilder = new StringBuilder();
                    while (rdr.Read())
                    {
                        stringBuilder.Append(rdr.GetString(0));
                    }
                    rdr.Close();
                    int count;

                    connection.Close();
                    if (int.TryParse(stringBuilder.ToString(), out count))
                    {
                        if (count > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log("실패");
                    Log(e.ToString());
                }
            }
            return false;
        }
        public void insertDataset(string _idx, string _datedata, int _weight,int _count,int _time, int _machineindex, int _exerciseclass, int _mucleclass)
        {
            using (MySqlConnection connection = ConnectionDB())
            {
                string insertQuery = string.Format("use {0};insert into tabledataset(Datedata,weight,count, time, machineindex, exerciseclass, muscleclass) value ({1},{2},{3},{4},{5},{6},{7});", "kks", "\"0000\"", _weight, _count, _time, _machineindex, _exerciseclass, _mucleclass);
                Log(insertQuery.ToString());
                try
                {

                    connection.Open();
                    MySqlCommand command = new MySqlCommand(insertQuery, connection);
                    if (command.ExecuteNonQuery() == -1)
                    {
                        Log("테이블 생성 실패");
                    }
                    else
                    {
                        Log("테이블 생성 성공");
                    }

                    connection.Close();
                }
                catch (Exception e)
                {
                    Log("실패");
                    Log(e.ToString());
                }
            }
        }
        public void UpdateDataset(string _idx, string _datedata, int _weight, int _count, int _time, int _machineindex, int _exerciseclass, int _mucleclass)
        {
            if(CheckDataset(_idx, _datedata, _weight, _machineindex))
            {
                using (MySqlConnection connection = ConnectionDB())
                {
                    string insertQuery = string.Format("use {0};Update tabledataset set count = count+{1} where datedata = \"{2}\"and weight ={3} and machineindex = {4};",_idx,_count, _datedata ,_weight,_machineindex);
                    Log(insertQuery.ToString());
                    try
                    {

                        connection.Open();
                        MySqlCommand command = new MySqlCommand(insertQuery, connection);
                        if (command.ExecuteNonQuery() == 1)
                        {
                            Log("데이터 업데이트 성공");
                        }
                        else
                        {
                            Log("데이터 업데이트 실패");
                        }

                        connection.Close();
                    }
                    catch (Exception e)
                    {
                        Log("실패");
                        Log(e.ToString());
                    }
                }
            }
            else
            {
                insertDataset(_idx, _datedata, _weight, _count, _time, _machineindex, _exerciseclass, _mucleclass);
            }
        }
    }
}
