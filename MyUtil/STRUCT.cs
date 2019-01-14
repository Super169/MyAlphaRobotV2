using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyUtil
{
    public class STRUCT
    {
        public enum TYPE
        {
            BOOL, BYTE, INT8, INT16, INT32, INT64, UINT8, UINT16, UINT32, UINT64, CHAR, STRUCT
        }

        private static int SizeOf(TYPE type)
        {
            switch (type)
            {
                case TYPE.BYTE:
                case TYPE.INT8:
                case TYPE.UINT8:
                case TYPE.CHAR:
                case TYPE.BOOL:
                    return 1;
                case TYPE.INT16:
                case TYPE.UINT16:
                    return 2;
                case TYPE.INT32:
                case TYPE.UINT32:
                    return 4;
                case TYPE.INT64:
                case TYPE.UINT64:
                    return 8;
                case TYPE.STRUCT:
                    return 0;

            }
            throw new System.Exception(String.Format("STRUCT::SizeOf - Unexpected type: {0}", type.ToString()));
        }

        private class FIELD
        {
            public int index;
            public int offset;
            public string name;
            public TYPE type;
            public int count;
            public int objSize;
            public readonly int size;
            private byte[] buffer;
            STRUCT child;

            public byte[] dataBuffer
            {
                get
                {
                    if (type == TYPE.STRUCT) return child.buffer;
                    return buffer;
                }
            }

            public FIELD(int index, int offset, string name, STRUCT child)
            {
                this.index = index;
                this.offset = offset;
                this.name = name;
                this.type = TYPE.STRUCT;
                this.count = 1;
                this.objSize = child.size;
                size = objSize;
                this.child = child;
            }

            public FIELD(bool objAlignment, int index, int offset, string name, TYPE type, int count, int objSize = 0)
            {
                objSize = (objSize <= 0 ? SizeOf(type) : objSize);
                if (objAlignment && (objSize > 1))
                {
                    int remain = offset % objSize;
                    if (remain > 0)
                    {
                        offset += objSize - remain;
                    }
                }

                this.index = index;
                this.offset = offset;
                this.name = name;
                this.type = type;
                this.count = count;
                this.objSize = objSize;
                size = count * this.objSize;
                buffer = new byte[size];
            }

            private enum EX_TYPE
            {
                INVALID_OPERATION, INVALID_TYPE, OUT_OF_INDEX, CONVERSION_FAILED, OVERFLOW
            }

            private Exception MyException(EX_TYPE ex, string op, object parm = null, object info = null)
            {
                String msg = "";
                switch (ex)
                {
                    case EX_TYPE.INVALID_OPERATION:
                        msg = "Invalid operation";
                        break;
                    case EX_TYPE.INVALID_TYPE:
                        msg = "Invalid type";
                        break;
                    case EX_TYPE.OUT_OF_INDEX:
                        msg = String.Format("Out of index, size: {0}", count);
                        break;
                    case EX_TYPE.CONVERSION_FAILED:
                        msg = String.Format("Conversion failed {0}", info);
                        break;
                    default:
                        msg = string.Format("Unknown exception {0}", ex.ToString());
                        break;
                }
                String header = String.Format("STRUCT:: {0}[{1}].{2}({3}) - ", this.name, type.ToString(), op, parm);
                return new Exception(string.Format("STRUCT:: {0}[{1}].{2}({3}) - {4}", this.name, type.ToString(), op, parm, msg));
            }

            public bool IsValid(Int64 i64, TypeCode tc)
            {
                switch (tc)
                {
                    case TypeCode.Byte:
                        if ((i64 < 0) || (i64 >= (1 << 8))) return false;
                        break;
                    case TypeCode.UInt16:
                        if ((i64 < 0) || (i64 >= (1 << 16))) return false;
                        break;
                    case TypeCode.UInt32:
                        if ((i64 < 0) || (i64 >= Math.Pow(2, 32))) return false;
                        break;
                    case TypeCode.UInt64:
                        if (i64 < 0) return false;
                        break;
                    case TypeCode.SByte:
                        if ((i64 < -(1 << 7)) || (i64 >= (1 << 7))) return false;
                        break;
                    case TypeCode.Int16:
                        if ((i64 < -(1 << 15)) || (i64 >= (1 << 15))) return false;
                        break;
                    case TypeCode.Int32:
                        if ((i64 < -Math.Pow(2, 31)) || (i64 >= Math.Pow(2, 31))) return false;
                        break;
                    case TypeCode.Int64:
                        break;
                    default:
                        return false;
                }
                return true;
            }

            public bool IsValid(Int64 i64, TYPE type)
            {
                switch (type)
                {
                    case TYPE.BYTE:
                        return IsValid(i64, TypeCode.Byte);
                    case TYPE.UINT16:
                        return IsValid(i64, TypeCode.UInt16);
                    case TYPE.UINT32:
                        return IsValid(i64, TypeCode.UInt32);
                    case TYPE.UINT64:
                        return IsValid(i64, TypeCode.UInt64);
                    case TYPE.INT8:
                        if ((i64 < -128) || (i64 >= 127)) return false;
                        return true;
                    case TYPE.INT16:
                        return IsValid(i64, TypeCode.Int16);
                    case TYPE.INT32:
                        return IsValid(i64, TypeCode.Int32);
                    case TYPE.INT64:
                        return true;
                }
                return false;
            }

            public bool IsNumeric()
            {
                return ((type == TYPE.BYTE) ||
                         (type == TYPE.UINT8) || (type == TYPE.UINT16) || (type == TYPE.UINT32) || (type == TYPE.UINT64) ||
                         (type == TYPE.INT8) || (type == TYPE.INT16) || (type == TYPE.INT32) || (type == TYPE.INT64));
            }

            public bool GetBool(int index = 0)
            {
                if (type != TYPE.BOOL)
                {
                    throw MyException(EX_TYPE.INVALID_OPERATION, "GetBool");
                }
                if (index >= this.count)
                {
                    throw MyException(EX_TYPE.OUT_OF_INDEX, "GetBool", string.Format("{0}", index));
                }
                return (buffer[index] != 0);
            }

            public void SetBool(bool value, int index)
            {
                if (type != TYPE.BOOL)
                {
                    throw MyException(EX_TYPE.INVALID_OPERATION, "GetBool");
                }
                if (index >= this.count)
                {
                    throw MyException(EX_TYPE.OUT_OF_INDEX, "SetBool", string.Format("{0},{1}", value, index));
                }
                buffer[index] = (byte)(value ? 1 : 0);
            }

            public char GetChar(int index = 0)
            {
                if (type != TYPE.CHAR)
                {
                    throw MyException(EX_TYPE.INVALID_OPERATION, "GetChar");
                }
                if (index >= this.count)
                {
                    throw MyException(EX_TYPE.OUT_OF_INDEX, "GetChar", string.Format("{0}", index));
                }
                return Convert.ToChar(buffer[index]);
            }

            public String GetString()
            {
                if (type != TYPE.CHAR)
                {
                    throw MyException(EX_TYPE.INVALID_OPERATION, "GetString");
                }
                return System.Text.Encoding.UTF8.GetString(buffer);
            }

            public void SetString(string value)
            {
                if (type != TYPE.CHAR)
                {
                    throw MyException(EX_TYPE.INVALID_OPERATION, "SetString");
                }
                byte[] data = System.Text.Encoding.UTF8.GetBytes(value);
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (i < data.Length ? data[i] : (byte)0);
                }
            }

            public T GetNumeric<T>(int index = 0)
            {
                T value = default(T);
                bool success = false;
                Int64 i64 = 0;
                UInt64 ui64 = 0;
                TypeCode tc = Type.GetTypeCode(typeof(T));

                if (index >= this.count)
                {
                    throw MyException(EX_TYPE.OUT_OF_INDEX, "GetNumeric", string.Format("{0}", index));
                }
                int pos = index * objSize;

                switch (type)
                {
                    // Unsigned data type
                    case TYPE.BYTE:
                    case TYPE.UINT8:
                        i64 = buffer[pos];
                        success = true;
                        break;
                    case TYPE.UINT16:
                        i64 = BitConverter.ToUInt16(buffer, pos);
                        success = true;
                        break;
                    case TYPE.UINT32:
                        i64 = BitConverter.ToUInt32(buffer, pos);
                        success = true;
                        break;
                    case TYPE.UINT64:
                        // Special handle for UInt64, as it cannot keep the value in i64
                        ui64 = BitConverter.ToUInt64(buffer, pos);
                        if (tc == TypeCode.UInt64)
                        {
                            value = (T)(object)Convert.ChangeType(ui64, typeof(T));
                            return value;
                        }
                        if (ui64 >= (2 ^ 63))
                        {
                            throw MyException(EX_TYPE.CONVERSION_FAILED, "GetNumeric", string.Format("{0}", index),
                                             string.Format("Target: {0}, value: {1}", tc.ToString(), ui64));
                        }
                        i64 = (Int64)ui64;
                        success = true;
                        break;
                    case TYPE.INT8:
                        int temp = buffer[pos];
                        i64 = (temp > 127 ? temp - 256 : temp);
                        success = true;
                        break;
                    case TYPE.INT16:
                        i64 = BitConverter.ToInt16(buffer, pos);
                        success = true;
                        break;
                    case TYPE.INT32:
                        i64 = BitConverter.ToInt32(buffer, pos);
                        success = true;
                        break;
                    case TYPE.INT64:
                        i64 = BitConverter.ToInt64(buffer, pos);
                        success = true;
                        break;
                    default:
                        throw MyException(EX_TYPE.INVALID_OPERATION, "GetNumeric");
                }
                if (!success)
                {
                    // should never reach here
                    throw MyException(EX_TYPE.CONVERSION_FAILED, "GetNumeric", string.Format("{0}", index));
                }

                if (!IsValid(i64, tc))
                {
                    throw MyException(EX_TYPE.CONVERSION_FAILED, "GetNumeric", string.Format("{0}", index),
                                      string.Format("Target: {0}, value: {1}", tc.ToString(), i64));
                }
                value = (T)(object)Convert.ChangeType(i64, typeof(T));
                return value;
            }

            public void SetNumeric(object value, int index = 0)
            {
                if (index >= this.count)
                {
                    throw MyException(EX_TYPE.OUT_OF_INDEX, "SetNumeric", string.Format("{0},{1}", value, index));
                }
                int pos = index * objSize;
                bool invalidType = false;
                byte[] data = null;
                try
                {
                    switch (type)
                    {
                        case TYPE.BYTE:
                        case TYPE.UINT8:
                            data = BitConverter.GetBytes(Convert.ToByte(value));
                            break;
                        case TYPE.UINT16:
                            UInt16 ui16 = Convert.ToUInt16(value);
                            data = BitConverter.GetBytes(ui16);
                            break;
                        case TYPE.UINT32:
                            UInt32 ui32 = Convert.ToUInt32(value);
                            data = BitConverter.GetBytes(ui32);
                            break;
                        case TYPE.UINT64:
                            UInt64 ui64 = Convert.ToUInt64(value);
                            data = BitConverter.GetBytes(ui64);
                            break;
                        case TYPE.INT8:
                            SByte sb = Convert.ToSByte(value);
                            data = BitConverter.GetBytes(sb);
                            break;
                        case TYPE.INT16:
                            Int16 i16 = Convert.ToInt16(value);
                            data = BitConverter.GetBytes(i16);
                            break;
                        case TYPE.INT32:
                            Int32 i32 = Convert.ToInt32(value);
                            data = BitConverter.GetBytes(i32);
                            break;
                        case TYPE.INT64:
                            Int64 i64 = Convert.ToInt64(value);
                            data = BitConverter.GetBytes(i64);
                            break;
                        default:
                            invalidType = true;
                            break;
                    }
                }
                catch (Exception)
                {
                    throw MyException(EX_TYPE.CONVERSION_FAILED, "SetNumeric", string.Format("{0},{1}", value, index));
                }
                if (invalidType)
                {
                    throw MyException(EX_TYPE.INVALID_OPERATION, "SetNumeric");
                }
                Array.Copy(data, 0, buffer, pos, objSize);
            }

            public STRUCT GetSTRUCT()
            {
                return child;
            }

            public T GetValue<T>(int index = 0)
            {
                TypeCode tc = Type.GetTypeCode(typeof(T));
                switch (tc)
                {
                    case TypeCode.Boolean:
                        return (T)(object)Convert.ChangeType(GetBool(index), typeof(T));
                    case TypeCode.Char:
                        return (T)(object)Convert.ChangeType(GetChar(index), typeof(T));
                    case TypeCode.String:
                        return (T)(object)Convert.ChangeType(GetString(), typeof(T));
                    case TypeCode.Byte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        return (T)(object)Convert.ChangeType(GetNumeric<T>(index), typeof(T));
                }
                throw new Exception(string.Format("STRUCT::GetValue() - Invalid type: {0}", tc));
            }

            public void SetValue(object value, int index = 0)
            {
                if (index >= this.count)
                {
                    throw MyException(EX_TYPE.OUT_OF_INDEX, "SetValue", string.Format("{0},{1}", value, index));
                }
                switch (type)
                {
                    case TYPE.BOOL:
                        SetBool((bool)value, index);
                        return;
                    case TYPE.CHAR:
                        SetString((string)value);
                        return;
                    case TYPE.BYTE:
                    case TYPE.INT8:
                    case TYPE.UINT8:
                    case TYPE.INT16:
                    case TYPE.UINT16:
                    case TYPE.INT32:
                    case TYPE.UINT32:
                    case TYPE.INT64:
                    case TYPE.UINT64:
                        SetNumeric(value, index);
                        return;
                }
                throw new Exception(string.Format("SetValue::GetValue() - Invalid type: {0}", type));
            }

            public void FillBuffer(byte[] source, int offset)
            {
                if (type == TYPE.STRUCT)
                {
                    child.LoadBuffer(source, offset);
                } else
                {
                    Array.Copy(source, offset, buffer, 0, size);
                }
            }
        }

        private bool _objAlignment;
        private List<FIELD> _fields;

        public byte[] buffer { get; private set; }
        public int size { get; private set; }
        public int fieldCount { get { return _fields.Count; } }
        public bool objAlignment { get { return _objAlignment; } }

        public STRUCT(bool objAlignment)
        {
            _fields = new List<FIELD>();
            size = 0;
            _objAlignment = objAlignment;
        }

        public STRUCT(STRUCT source)
        {
            _fields = new List<FIELD>();
            size = 0;
            _objAlignment = source.objAlignment;
            Clone(source, false);
        }

        public void Clone(STRUCT source, bool copyValue)
        {
            _objAlignment = source.objAlignment;
            int count = source._fields.Count;
            // For safety, search index instead of foreach to make sure fields are added in sequent
            for (int index = 0; index < count; index++)
            {
                FIELD fSource = source._fields.Find(x => x.index == index);
                if (fSource == null)
                {
                    throw new Exception(string.Format("STRUCT::clone - missinge field with index {0}", index));
                }
                FIELD fNew = new FIELD(_objAlignment, index, size, fSource.name, fSource.type, fSource.count);
                if (copyValue)
                {
                    Array.Copy(fSource.dataBuffer, fNew.dataBuffer, fNew.size);
                }
                _fields.Add(fNew);
                size = fNew.offset + fNew.size;
            }
        }

        public bool Copy(STRUCT source, bool allowMissing)
        {
            Clear();
            bool success = true;
            foreach (FIELD f in _fields)
            {
                if (f.name != "")
                {
                    if (source.Exists(f.name))
                    {
                        FIELD fs = source.GetField(f.name);
                        if ((f.type == TYPE.CHAR) && (fs.type == TYPE.CHAR))
                        {
                            f.SetString(fs.GetString());
                        }
                        else if (((f.type == TYPE.BOOL) && (fs.type == TYPE.BOOL)) ||
                                 (f.IsNumeric() && fs.IsNumeric()))
                        {
                            try
                            {
                                int maxCount = Math.Min(f.count, fs.count);
                                for (int i = 0; i < maxCount; i++)
                                {
                                    if (f.type == TYPE.BOOL)
                                    {
                                        f.SetBool(fs.GetBool(i), i);
                                    }
                                    else if (fs.type == TYPE.UINT64)
                                    {
                                        // have to use UInt64 to handle UINT64, otherwise can use INT64 for all other cases
                                        f.SetNumeric(fs.GetNumeric<UInt64>(i), i);
                                    }
                                    else
                                    {
                                        f.SetNumeric(fs.GetNumeric<Int64>(i), i);
                                    }
                                }
                            } catch (Exception)
                            {
                                success = false;
                                break;
                            }
                        }
                        else
                        {
                            success = false;
                            break;
                        }
                    }
                    else
                    {
                        if (!allowMissing)
                        {
                            success = false;
                            break;
                        }
                    }
                }
            }
            if (!success) Clear();
            return success;
        }

        public bool AddField(string name, TYPE type, int count = 1)
        {
            if (count <= 0) return false;
            name = name.Trim();
            if ((name != "") && (_fields.Exists(x => x.name == name)))
            {
                throw new System.Exception(string.Format("STRUCT::AddField - Filed {0} already exists", name));
            }
            FIELD f = new FIELD(_objAlignment, _fields.Count, size, name, type, count);
            _fields.Add(f);
            // size += f.size;  // for object alignment, the offset can be changed
            size = f.offset + f.size;
            return true;
        }

        public bool InitBuffer()
        {
            if (size == 0) return false;
            buffer = new byte[size];
            Array.Clear(buffer, 0, size);
            return true;
        }

        public string Info()
        {
            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < _fields.Count; index++)
            {
                FIELD f = _fields.Find(x => x.index == index);
                if (f != null)
                {
                    sb.Append(string.Format("{0:0000} - {1,-10} : ", f.offset, f.name));
                    string temp = f.type.ToString();
                    if (f.count > 1)
                    {
                        temp += string.Format("[{0}]", f.count);
                    }
                    sb.Append(string.Format("{0,-12} : ", temp));
                    string value = "";
                    switch (f.type)
                    {
                        case TYPE.BOOL:
                            for (int i = 0; i < f.count; i++)
                            {
                                value += f.GetBool(i).ToString() + " ";
                            }
                            break;
                        case TYPE.CHAR:
                            value = f.GetString();
                            break;
                        case TYPE.BYTE:
                            for (int i = 0; i < f.count; i++)
                            {
                                value += string.Format("0x{0:X2} ", f.GetNumeric<byte>(i));
                            }
                            break;
                        case TYPE.UINT64:
                            for (int i = 0; i < f.count; i++)
                            {
                                value += f.GetNumeric<UInt64>(i).ToString() + " ";
                            }
                            break;
                        case TYPE.UINT8:
                        case TYPE.UINT16:
                        case TYPE.UINT32:
                        case TYPE.INT8:
                        case TYPE.INT16:
                        case TYPE.INT32:
                        case TYPE.INT64:
                            for (int i = 0; i < f.count; i++)
                            {
                                value += f.GetNumeric<Int64>(i).ToString() + " ";
                            }
                            break;
                        default:
                            value = "{unknown}";
                            break;
                    }
                    sb.AppendLine(value);
                }
            }
            return sb.ToString();
        }

        public bool Exists(string name)
        {
            name = name.Trim();
            return _fields.Exists(x => x.name == name);
        }

        private FIELD GetField(string name)
        {
            name = name.Trim();
            if (name == "")
            {
                throw new Exception(string.Format("STRUCT::GetNumeric - Missing file name"));
            }

            FIELD f = _fields.Find(x => x.name == name);
            if (f == null)
            {
                throw new Exception(string.Format("STRUCT::GetNumeric - Filed not found: {0}", name));
            }
            return f;
        }

        public bool GetBool(string name, int index = 0)
        {
            FIELD f = GetField(name);
            return f.GetBool(index);
        }

        public void SetBool(string name, bool value, int index = 0)
        {
            FIELD f = GetField(name);
            f.SetBool(value, index);
        }

        public char GetChar(string name, int index = 0)
        {
            FIELD f = GetField(name);
            return f.GetChar(index);
        }

        public string GetString(string name)
        {
            FIELD f = GetField(name);
            return f.GetString();
        }

        public void SetString(string name, string value)
        {
            FIELD f = GetField(name);
            f.SetString(value);
        }

        public T GetNumeric<T>(string name, int index = 0)
        {
            FIELD f = GetField(name);
            return f.GetNumeric<T>(index);
        }

        public bool SetNumeric(string name, object value, int index = 0)
        {
            FIELD f = GetField(name);
            f.SetNumeric(value, index);
            return true;
        }

        public T GetValue<T>(string name, int index = 0)
        {
            FIELD f = GetField(name);
            return f.GetValue<T>(index);
        }

        public void SetValue(string name, object value, int index = 0)
        {
            FIELD f = GetField(name);
            f.SetValue(value, index);
        }

        private void ResetFields()
        {
            foreach (FIELD f in _fields)
            {
                // Array.Copy(buffer, f.offset, f.dataBuffer, 0, f.size);
                f.FillBuffer(buffer, f.offset);
            }
        }

        public void Clear()
        {
            InitBuffer();
            ResetFields();
        }

        public bool LoadBuffer(byte[] data, int offset = 0)
        {
            if (offset < 0) throw new Exception("STRUCT::LoadBuffer - Invalid offset");
            InitBuffer();
            if ((size <= 0) || (buffer == null) || (buffer.Length == 0) || (buffer.Length <= offset))
            {
                throw new System.Exception("STRUCT::LoadBuffer - No data field defined");
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (i < data.Length ? data[offset + i] : (byte)0);
            }
            ResetFields();
            return true;
        }

        public void ReBuildBuffer()
        {
            InitBuffer();
            if ((size <= 0) || (buffer == null) || (buffer.Length == 0))
            {
                throw new System.Exception("STRUCT::Rebuild - No data field defined");
            }
            foreach (FIELD f in _fields)
            {
                Array.Copy(f.dataBuffer, 0, buffer, f.offset, f.size);
            }
        }

    }
}
