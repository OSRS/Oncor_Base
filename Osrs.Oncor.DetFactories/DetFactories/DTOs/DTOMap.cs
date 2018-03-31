using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs
{
    public class DTOMap<DTO> where DTO : BaseDTO, IKeyed
    {
        private readonly Dictionary<string, DTO> _map = new Dictionary<string, DTO>();

        public bool Add(DTO dto)
        {
            bool success = false;
            if (dto.LookupKey!=null && !_map.ContainsKey(dto.LookupKey))
            {
                _map.Add(dto.LookupKey, dto);
                success = true;
            }
            return success;
        }

        public int Count => _map.Count;

        public IList<string> Keys => new List<string>(_map.Keys);
        public IList<DTO> Values => new List<DTO>(_map.Values);
        public DTO this[string key] => _map[key];
        public bool ContainsKey(string key)
        {
            return _map.ContainsKey(key);
        }
    }
}
