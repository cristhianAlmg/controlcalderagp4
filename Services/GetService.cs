using BlazorApp1.Data;

namespace BlazorApp1.Services
{
    public class GetService
    {
        private static DataGet dataGet = new();

        /*
        public async Task<bool> FormatedData(DataGet newData)
        {
            dataGet = newData;
            return true;
        }
        */

        public async Task<DataGet> GetAll()
        {
            return Get();
        }


        public DataGet Get()
        {
            DataGet data = new()
            {
                Temperature = "20°",
                Time = "07:00:00"
            };

            return data;
        }
    }
}
