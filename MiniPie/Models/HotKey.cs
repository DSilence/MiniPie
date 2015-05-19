using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MiniPie.Core.HotKeyManager;

namespace MiniPie.Models
{
    public struct HotKey
    {
        public Key Key { get; set; }
        public KeyModifier KeyModifier { get; set; }

        #region EqualityMembers
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is HotKey && Equals((HotKey) obj);
        }

        public bool Equals(HotKey other)
        {
            return Key == other.Key && KeyModifier == other.KeyModifier;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Key * 397) ^ (int)KeyModifier;
            }
        }
        #endregion
    }
}
