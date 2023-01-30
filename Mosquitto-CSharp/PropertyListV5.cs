using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MosquittoPropertyPtr = System.IntPtr;

namespace Mosquitto
{
    public struct PropertyListV5
    {
        internal MosquittoPropertyPtr nativePropertyList;

        internal PropertyListV5(MosquittoPropertyPtr prop)
        {
            nativePropertyList = prop;
        }

        public PropertyListV5 next
        {
            get
            {
                return new PropertyListV5(Native.mosquitto_property_next(nativePropertyList));
            }
        }

        /// <summary>
        /// Try to find and read the byte value of the given property
        /// </summary>
        /// <param name="property">The property to read from our list</param>
        /// <param name="value">The output value if it was found</param>
        /// <returns>True if the property was found in this property list</returns>
        public bool TryRead(PropertyV5 property, out byte value)
        {
            value = default;
            return Native.mosquitto_property_read_byte(nativePropertyList, (Native.mqtt5_property)property, ref value, false) != IntPtr.Zero;
        }

        /// <summary>
        /// Read all instances of this property and append them to the passed in list. 
        /// List will be constructed if passed in as null and at least one instance of the property was found.
        /// </summary>
        /// <param name="property">The property to search for in our property list</param>
        /// <param name="values">A list that will be populated from the results of the found property</param>
        public void ReadAll(PropertyV5 property, ref List<byte> values)
        {
            byte val = default;
            IntPtr p = Native.mosquitto_property_read_byte(nativePropertyList, (Native.mqtt5_property)property, ref val, false);

            while(p != IntPtr.Zero)
            {
                if (values == null)
                {
                    values = new List<byte>();
                }
                values.Add(val);
                p = Native.mosquitto_property_read_byte(p, (Native.mqtt5_property)property, ref val, true);
            }
        }

        /// <summary>
        /// Try to find and read the ushort value of the given property
        /// </summary>
        /// <param name="property">The property to read from our list</param>
        /// <param name="value">The output value if it was found</param>
        /// <returns>True if the property was found in this property list</returns>
        public bool TryRead(PropertyV5 property, out ushort value)
        {
            value = default;
            return Native.mosquitto_property_read_int16(nativePropertyList, (Native.mqtt5_property)property, ref value, false) != IntPtr.Zero;
        }

        /// <summary>
        /// Read all instances of this property and append them to the passed in list. 
        /// List will be constructed if passed in as null and at least one instance of the property was found.
        /// </summary>
        /// <param name="property">The property to search for in our property list</param>
        /// <param name="values">A list that will be populated from the results of the found property</param>
        public void ReadAll(PropertyV5 property, ref List<ushort> values)
        {
            ushort val = default;
            IntPtr p = Native.mosquitto_property_read_int16(nativePropertyList, (Native.mqtt5_property)property, ref val, false);

            while (p != IntPtr.Zero)
            {
                if (values == null)
                {
                    values = new List<ushort>();
                }
                values.Add(val);
                p = Native.mosquitto_property_read_int16(p, (Native.mqtt5_property)property, ref val, true);
            }
        }

        /// <summary>
        /// Try to find and read the uint value of the given property
        /// </summary>
        /// <param name="property">The property to read from our list</param>
        /// <param name="value">The output value if it was found</param>
        /// <returns>True if the property was found in this property list</returns>
        public bool TryRead(PropertyV5 property, out uint value)
        {
            value = default;
            return property == PropertyV5.SubscriptionIdentifier
                ? Native.mosquitto_property_read_varint(nativePropertyList, (Native.mqtt5_property)property, ref value, false) != IntPtr.Zero
                : Native.mosquitto_property_read_int32(nativePropertyList, (Native.mqtt5_property)property, ref value, false) != IntPtr.Zero;
        }

        /// <summary>
        /// Read all instances of this property and append them to the passed in list. 
        /// List will be constructed if passed in as null and at least one instance of the property was found.
        /// </summary>
        /// <param name="property">The property to search for in our property list</param>
        /// <param name="values">A list that will be populated from the results of the found property</param>
        public void ReadAll(PropertyV5 property, ref List<uint> values)
        {
            uint val = default;
            IntPtr p = property == PropertyV5.SubscriptionIdentifier
                ? Native.mosquitto_property_read_varint(nativePropertyList, (Native.mqtt5_property)property, ref val, false) 
                : Native.mosquitto_property_read_int32(nativePropertyList, (Native.mqtt5_property)property, ref val, false);

            while (p != IntPtr.Zero)
            {
                if (values == null)
                {
                    values = new List<uint>();
                }
                values.Add(val);
                p = property == PropertyV5.SubscriptionIdentifier
                ? Native.mosquitto_property_read_varint(p, (Native.mqtt5_property)property, ref val, true)
                : Native.mosquitto_property_read_int32(p, (Native.mqtt5_property)property, ref val, true);
            }
        }

        /// <summary>
        /// Try to find and read the string value of the given property
        /// </summary>
        /// <param name="property">The property to read from our list</param>
        /// <param name="value">The output value if it was found</param>
        /// <returns>True if the property was found in this property list</returns>
        public bool TryRead(PropertyV5 property, out string value)
        {
            value = default;
            IntPtr val = IntPtr.Zero;
            if (Native.mosquitto_property_read_string(nativePropertyList, (Native.mqtt5_property)property, ref val, false) == IntPtr.Zero)
            {
                return false;
            }
            value = Marshal.PtrToStringAnsi(val);
            Native.native_free(val);
            return true;
        }

        /// <summary>
        /// Read all instances of this property and append them to the passed in list. 
        /// List will be constructed if passed in as null and at least one instance of the property was found.
        /// </summary>
        /// <param name="property">The property to search for in our property list</param>
        /// <param name="values">A list that will be populated from the results of the found property</param>
        public void ReadAll(PropertyV5 property, ref List<string> values)
        {
            IntPtr val = IntPtr.Zero;
            IntPtr p = Native.mosquitto_property_read_string(nativePropertyList, (Native.mqtt5_property)property, ref val, false);

            while (p != IntPtr.Zero)
            {
                if (values == null)
                {
                    values = new List<string>();
                }
                values.Add(Marshal.PtrToStringAnsi(val));
                Native.native_free(val);
                p = Native.mosquitto_property_read_string(p, (Native.mqtt5_property)property, ref val, true);
            }
        }

        /// <summary>
        /// Try to find and read the string value of the given property
        /// </summary>
        /// <param name="property">The property to read from our list</param>
        /// <param name="value">The output value if it was found</param>
        /// <returns>True if the property was found in this property list</returns>
        public bool TryRead(PropertyV5 property, out KeyValuePair<string, string> value)
        {
            value = default;
            IntPtr key = IntPtr.Zero, val = IntPtr.Zero;
            if (Native.mosquitto_property_read_string_pair(nativePropertyList, (Native.mqtt5_property)property, ref key, ref val, false) == IntPtr.Zero)
            {
                return false;
            }

            value = new KeyValuePair<string, string>(Marshal.PtrToStringAnsi(key), Marshal.PtrToStringAnsi(val));
            Native.native_free(key);
            Native.native_free(val);
            return true;
        }

        /// <summary>
        /// Read all instances of this property and append them to the passed in dictionary. 
        /// Dictionary will be constructed if passed in as null and at least one instance of the property was found.
        /// </summary>
        /// <param name="property">The property to search for in our property list</param>
        /// <param name="values">A list that will be populated from the results of the found property</param>
        public void ReadAll(PropertyV5 property, ref Dictionary<string, string> values)
        {
            IntPtr key = IntPtr.Zero, val = IntPtr.Zero;
            IntPtr p = Native.mosquitto_property_read_string_pair(nativePropertyList, (Native.mqtt5_property)property, ref key, ref val, false);

            while (p != IntPtr.Zero)
            {
                if (values == null)
                {
                    values = new Dictionary<string, string>();
                }
                values.Add(Marshal.PtrToStringAnsi(key), Marshal.PtrToStringAnsi(val));
                Native.native_free(key);
                Native.native_free(val);
                p = Native.mosquitto_property_read_string_pair(p, (Native.mqtt5_property)property, ref key, ref val, true);
            }
        }

        /// <summary>
        /// Try to find and read the byte[] value of the given property
        /// </summary>
        /// <param name="property">The property to read from our list</param>
        /// <param name="value">The output value if it was found</param>
        /// <returns>True if the property was found in this property list</returns>
        public bool TryRead(PropertyV5 property, out byte[] value)
        {
            value = default;
            IntPtr val = IntPtr.Zero;
            ushort len = 0;
            if (Native.mosquitto_property_read_binary(nativePropertyList, (Native.mqtt5_property)property, ref val, ref len, false) == IntPtr.Zero)
            {
                return false;
            }
            value = Array.Empty<byte>();
            if (len > 0)
            {
                value = new byte[len];
                Marshal.Copy(val, value, 0, len);
            }
            Native.native_free(val);
            return true;

        }

        /// <summary>
        /// Read all instances of this property and append them to the passed in list. 
        /// List will be constructed if passed in as null and at least one instance of the property was found.
        /// </summary>
        /// <param name="property">The property to search for in our property list</param>
        /// <param name="values">A list that will be populated from the results of the found property</param>
        public void ReadAll(PropertyV5 property, ref List<byte[]> values)
        {
            IntPtr val = IntPtr.Zero;
            ushort len = 0;

            IntPtr p = Native.mosquitto_property_read_binary(nativePropertyList, (Native.mqtt5_property)property, ref val, ref len, false);

            while (p != IntPtr.Zero)
            {
                if (values == null)
                {
                    values = new List<byte[]>();
                }
                byte[] arr = Array.Empty<byte>();
                if (len > 0)
                {
                    arr = new byte[len];
                    Marshal.Copy(val, arr, 0, len);
                }
                values.Add(arr);
                Native.native_free(val);
                p = Native.mosquitto_property_read_binary(p, (Native.mqtt5_property)property, ref val, ref len, true);
            }
        }

    }

    public class ManagedPropertyListV5 : IDisposable
    {
        private PropertyListV5 _propertyList;
        private bool _disposed;

        public PropertyListV5 propertyList
        {
            get
            {
                return _propertyList;
            }
        }

        public ManagedPropertyListV5(PropertyListV5 properties)
        {
            SetProperties(in properties);
        }

        ~ManagedPropertyListV5()
        {
            Dispose(true);
        }

        public void SetProperties(in PropertyListV5 properties)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Tried to set properties after PropertyList was disposed");
            }
            Native.mosquitto_property_free_all(ref _propertyList.nativePropertyList);
            Native.mosquitto_property_copy_all(ref _propertyList.nativePropertyList, properties.nativePropertyList);
        }

        public Error Add(PropertyV5 property, byte value)
        {
            return (Error)Native.mosquitto_property_add_byte(ref _propertyList.nativePropertyList, (Native.mqtt5_property)property, value);
        }

        public Error Add(PropertyV5 property, ushort value)
        {
            return (Error)Native.mosquitto_property_add_int16(ref _propertyList.nativePropertyList, (Native.mqtt5_property)property, value);
        }


        public Error Add(PropertyV5 property, uint value)
        {
            return property == PropertyV5.SubscriptionIdentifier 
                ? (Error)Native.mosquitto_property_add_varint(ref _propertyList.nativePropertyList, (Native.mqtt5_property)property, value)
                : (Error)Native.mosquitto_property_add_int32(ref _propertyList.nativePropertyList, (Native.mqtt5_property)property, value);
        }

        public Error Add(PropertyV5 property, string value)
        {
            return (Error)Native.mosquitto_property_add_string(ref _propertyList.nativePropertyList, (Native.mqtt5_property)property, value);
        }
        public Error Add(PropertyV5 property, string key, string value)
        {
            return (Error)Native.mosquitto_property_add_string_pair(ref _propertyList.nativePropertyList, (Native.mqtt5_property)property, key, value);
        }
        public Error Add(PropertyV5 property, byte[] value, int length)
        {
            return (Error)Native.mosquitto_property_add_binary(ref _propertyList.nativePropertyList, (Native.mqtt5_property)property, value, (ushort)length);
        }

        public void Dispose()
        {
            Dispose(false);
        }

        public void Dispose(bool isFinalize)
        {
            _disposed = true;
            Native.mosquitto_property_free_all(ref _propertyList.nativePropertyList);
            if (!isFinalize)
            {
                GC.SuppressFinalize(this);
            }
        }

        public static implicit operator PropertyListV5(ManagedPropertyListV5 propertyList)
        {
            return propertyList != null ? propertyList.propertyList : default;
        }
    }

    public static class ManagedPropertyListV5Pool
    {
        private const int _maxPoolSize = 16;

        private static ConcurrentBag<ManagedPropertyListV5> _pool = new ConcurrentBag<ManagedPropertyListV5>();

        public static ManagedPropertyListV5 Obtain(in PropertyListV5 propertyList)
        {
            if (propertyList.nativePropertyList == IntPtr.Zero)
            {
                return null;
            }

            if (_pool.TryTake(out var plist))
            {
                plist.SetProperties(propertyList);
                return plist;
            }
            return new ManagedPropertyListV5(propertyList);
        }

        public static void Release(ref ManagedPropertyListV5 propertyList)
        {
            if (propertyList == null)
            {
                return;
            }

            if (_pool.Count >= _maxPoolSize)
            {
                propertyList.Dispose();
            }
            else
            {
                _pool.Add(propertyList);
            }
            propertyList = null;
        }

    }
}