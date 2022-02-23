using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DataProvider_Server_voucher
{
    class GM_DB
    {
        public GM_DB()
        {
            init();
        }
        public static GM_DB inst;
        public MySqlConnection ConnectionDB()
        {
            MySqlConnection connection = new MySqlConnection("Server=localhost;Port=3306;Database=voucher;UiD=root;Pwd=cogsci2017;");
            Console.WriteLine("Connect DB");
            return connection;
        }
        public void init()
        {
            if (inst != null)
            {
                inst = this;
                Console.WriteLine("This");
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
            Addcolumn(_idx, "DataPath", "fileDate", "varchar(255)");
            Addcolumn(_idx, "DataPath", "filePath", "varchar(30)");
            CreateTable(_idx, "Data");
            Addcolumn(_idx, "Data", "fileDate", "varchar(30)");
            Addcolumn(_idx, "Data", "machine", "varchar(30)");
            Addcolumn(_idx, "Data", "weight", "int");
            Addcolumn(_idx, "Data", "count", "int");
            CreateTable(_idx, "Machine");
            Addcolumn(_idx, "Machine", "fileDate", "varchar(30)");
            Addcolumn(_idx, "Machine", "Allout(Guess)", "varchar(30)");
            Addcolumn(_idx, "Machine", "weight", "int");
            Addcolumn(_idx, "Machine", "count", "int");
            Addcolumn(_idx, "Machine", "1RM", "int");
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
        /// <summary>
        /// _idx와 같은 이름을 가지고 있는 테이블의 갯수를 반환
        /// </summary>
        /// <param name="_idx">schema를 확인하기위한 schema의 이름</param>
        /// <returns></returns>
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
        /// <param name="_query"></param>
        /// <param name="_connection"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 디버그를 찍기 위한 함수
        /// </summary>
        /// <param name="_log">디버그 string</param>
        public void Log(string _log)
        {
            Console.WriteLine(_log);
        }
        /// <summary>
        /// DataPath테이블에서 datedata와 filename이 일치하는 값이 있는지 확인
        /// </summary>
        /// <param name="_idx">schema명</param>
        /// <param name="_dateData">찾으려 하는 datedata</param>
        /// <param name="_filename">찾으려 하는 filename</param>
        /// <returns>찾았는지에 대한 여부</returns>
        public bool CheckDataPathData(string _idx, string _dateData, string _filename)
        {
            using (MySqlConnection connection = ConnectionDB())
            {
                string insertQuery = string.Format("use {0}; SELECT COUNT(*) FROM DataPath WHERE filePath= \"{1}\" and datedata = \"{2}\";", _idx, _filename, _dateData);
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
        /// <summary>
        /// DataPath에 데이터 삽입 값이 존재할경우 추가 하지 않음.
        /// </summary>
        /// <param name="_idx">schema명</param>
        /// <param name="_dateData">넣으려 하는 datedata</param>
        /// <param name="_filename">넣으려 하는 filename</param>
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
        /// <summary>
        /// datapath에 데이터 삽입
        /// </summary>
        /// <param name="_idx">schema명</param>
        /// <param name="_dateData">넣으려 하는 datedata</param>
        /// <param name="_filename">넣으려 하는 filename</param>
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
        /// <summary>
        /// Dataset테이블에서 datedata와 weight, macineindex가 일치하는 값이 있는지 확인.
        /// </summary>
        /// <param name="_idx"></param>
        /// <param name="_datedata"></param>
        /// <param name="_weight"></param>
        /// <param name="_machineindex"></param>
        /// <returns>있다면 true 없다면 false</returns>
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
        /// <summary>
        /// Dataset테이블에 데이터 삽입
        /// </summary>
        /// <param name="_idx">schema 명</param>
        /// <param name="_datedata">날짜</param>
        /// <param name="_weight">무게</param>
        /// <param name="_count">횟수</param>
        /// <param name="_time">운동시간</param>
        /// <param name="_machineindex">머신의 index</param>
        /// <param name="_exerciseclass">운동종류</param>
        /// <param name="_mucleclass">운동에 쓰이는 근육</param>
        public void insertDataset(string _idx, string _datedata, int _weight, int _count, int _time, int _machineindex, int _exerciseclass, int _mucleclass)
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
        /// <summary>
        /// Dataset테이블에 데이터 업데이트 데이터가 있다면 횟수를 추가하고 없다면 새로 삽입
        /// </summary>
        /// <param name="_idx">schema 명</param>
        /// <param name="_datedata">날짜</param>
        /// <param name="_weight">무게</param>
        /// <param name="_count">횟수</param>
        /// <param name="_time">운동시간</param>
        /// <param name="_machineindex">머신의 index</param>
        /// <param name="_exerciseclass">운동종류</param>
        /// <param name="_mucleclass">운동에 쓰이는 근육</param>
        public void UpdateDataset(string _idx, string _datedata, int _weight, int _count, int _time, int _machineindex, int _exerciseclass, int _mucleclass)
        {
            if (CheckDataset(_idx, _datedata, _weight, _machineindex))
            {
                using (MySqlConnection connection = ConnectionDB())
                {
                    string insertQuery = string.Format("use {0};Update tabledataset set count = count+{1} where datedata = \"{2}\"and weight ={3} and machineindex = {4};", _idx, _count, _datedata, _weight, _machineindex);
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
        /// <summary>
        /// DataSet테이블의 데이터 반환
        /// </summary>
        /// <param name="_idx">schema명</param>
        /// <param name="_datedata">날짜</param>
        /// <param name="_machineindex">머신의 index</param>
        /// <returns>세미콜론으로 줄구분을하며 콤마로 값구분을함 날짜, 무게, 횟수, 시간, 머신인덱스, 운동종류, 운동에 쓰이는 근육순</returns>
        public string GetDataset(string _idx, string _datedata, int _machineindex)
        {
            using (MySqlConnection connection = ConnectionDB())
            {
                string insertQuery = string.Format("use {0}; SELECT * FROM tableDataset WHERE datedata= \"{1}\" and machineindex = {2};", _idx, _datedata, _machineindex);
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
                        stringBuilder.Append(",");
                        stringBuilder.Append(rdr.GetInt32(1));
                        stringBuilder.Append(",");
                        stringBuilder.Append(rdr.GetInt32(2));
                        stringBuilder.Append(",");
                        stringBuilder.Append(rdr.GetInt32(3));
                        stringBuilder.Append(",");
                        stringBuilder.Append(rdr.GetInt32(4));
                        stringBuilder.Append(",");
                        stringBuilder.Append(rdr.GetInt32(5));
                        stringBuilder.Append(",");
                        stringBuilder.Append(rdr.GetInt32(6));
                        stringBuilder.Append(";");
                    }
                    Log(stringBuilder.ToString());

                    rdr.Close();

                    connection.Close();
                    return stringBuilder.ToString();
                }
                catch (Exception e)
                {
                    Log("실패");
                    Log(e.ToString());
                    return null;
                }
            }
            return null;
        }
        /// <summary>
        /// datapath테이블의 날짜가 일치하는 모든데이터 반환
        /// </summary>
        /// <param name="_idx">schema명</param>
        /// <param name="_datedata">날짜</param>
        /// <returns>세미콜론으로 줄구분을하며 콤마로 값구분을함 날짜, 파일이름순</returns>
        public string GetDataPath(string _idx, string _datedata)
        {
            using (MySqlConnection connection = ConnectionDB())
            {
                string insertQuery = string.Format("use {0}; SELECT * FROM datapath WHERE datedata= \"{1}\"", _idx, _datedata);
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
                        stringBuilder.Append(",");
                        stringBuilder.Append(rdr.GetString(1));
                        stringBuilder.Append(";");
                    }
                    Log(stringBuilder.ToString());

                    rdr.Close();

                    connection.Close();
                    return stringBuilder.ToString();
                }
                catch (Exception e)
                {
                    Log("실패");
                    Log(e.ToString());
                    return null;
                }
            }
            return null;
        }
    }
}
