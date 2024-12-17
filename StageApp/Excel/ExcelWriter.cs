namespace StageApp.Excel
{
    public class ExcelWriter
    {
        public static void SetExcel(string filePath, List<string[]> data) // maakt CSV bestanden ipv Excel bestanden maar excel ondersteunt csv
        {
            using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                foreach (var row in data)
                {
                    var line = string.Join(",", row);
                    writer.WriteLine(line);
                }
            }
        }
    }
}
