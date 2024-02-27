using Application.Products;
using Newtonsoft.Json;


namespace Shop
{
    public static class TExtension
    {
        public static void SetT<TItemViewModel>(this ISession session, string key, List<TItemViewModel> value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static List<TItemViewModel> GetT<TItemViewModel>(this ISession session, string key)
        {
            var value = session.GetString(key);
            var result = value != null ? JsonConvert.DeserializeObject<List<TItemViewModel>>(value) : null;
            return result;
        }
    }
}
