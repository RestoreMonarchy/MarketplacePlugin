using Newtonsoft.Json;
using Rocket.Core.Logging;
using System.IO;

namespace UnturnedMarketplacePlugin.Storage
{
    public class DataStorage<T> where T : class
    {
        public string DataPath { get; private set; }
        public DataStorage(string dir, string fileName)
        {
            DataPath = Path.Combine(dir, fileName);
        }

        public void Save(T obj)
        {
            string objData = JsonConvert.SerializeObject(obj, Formatting.Indented);

            using (StreamWriter stream = new StreamWriter(DataPath, false))
            {
                stream.Write(objData);
            }
        }

        ///<summary>We want to pass exception to the caller, therefore T is out parameter and return is boolean. 
        ///When caller gets false then you may want to unload plugin.</summary>
        public T Read()
        {
            if (File.Exists(DataPath))
            {
                string dataText;
                using (StreamReader stream = File.OpenText(DataPath))
                {
                    dataText = stream.ReadToEnd();                    
                }
                try
                {
                    return JsonConvert.DeserializeObject<T>(dataText);
                }
                catch (JsonException e)
                {
                    Logger.LogError(e.Message);
                    return null;
                }
            } else
            {
                return null;
            }
        }
    }
}
