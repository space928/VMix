using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VMix
{
    //Paramters allow us to encapsulate basic types such that changes in these values can be tracked and stored in scenes

    public abstract class Parameter : BindableBase, ICloneable, IEquatable<Parameter>
    {
        private bool enabled = true;
        public bool Enabled
        {
            get { return enabled; }
            set { SetProperty(ref this.enabled, value); }
        }
        private bool multipleValues = false;
        public bool MultipleValues
        {
            get { return multipleValues; }
            set { SetProperty(ref this.multipleValues, value); }
        }

        public Parameter()
        {

        }

        public Parameter(Parameter src)
        {
            enabled = src.enabled;
            multipleValues = src.multipleValues;
        }

        public abstract object Clone();

        public abstract void CopyValueFrom(Parameter p);

        public bool Equals(Parameter other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }
    }

    public class DoubleParameter : Parameter
    {
        private double value = 0;
        private double min = double.MinValue;
        private double max = double.MaxValue;

        public static readonly DoubleParameter multipleValueParameter = new DoubleParameter() { MultipleValues = true };

        public double Value
        {
            get { return value; }
            set { SetProperty(ref this.value, value); }
        }
        public double Min
        {
            get { return min; }
            set { SetProperty(ref min, value); }//this.min = value; }
        }
        public double Max
        {
            get { return max; }
            set { SetProperty(ref max, value); }
        }

        public DoubleParameter()
        {
            
        }

        public DoubleParameter(double value)
        {
            this.value = value;
        }

        public DoubleParameter(double value, double min, double max)
        {
            this.value = value;
            this.max = max;
            this.min = min;
        }

        public DoubleParameter(DoubleParameter src) : base(src)
        {
            this.value = src.value;
            this.max = src.max;
            this.min = src.min;
        }

        public static DoubleParameter operator +(DoubleParameter a, DoubleParameter b)
        {
            return new DoubleParameter { value = a.value + b.value, min = Math.Max(a.min, b.min), max = Math.Max(a.max, b.max) };
        }
        public static DoubleParameter operator +(DoubleParameter a, double b)
        {
            return new DoubleParameter { value = a.value + b, min = a.min, max = a.max };
        }
        public static DoubleParameter operator *(DoubleParameter a, DoubleParameter b)
        {
            return new DoubleParameter { value = a.value * b.value, min = Math.Max(a.min, b.min), max = Math.Max(a.max, b.max) };
        }
        public static DoubleParameter operator *(DoubleParameter a, double b)
        {
            return new DoubleParameter { value = a.value * b, min = a.min, max = a.max };
        }
        public static implicit operator double(DoubleParameter a)
        {
            return a?.value??0;
        }
        public static implicit operator DoubleParameter(double a)
        {
            return new DoubleParameter(a);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode() + Enabled.GetHashCode();
        }

        public override object Clone()
        {
            return new DoubleParameter(this);
        }

        public override void CopyValueFrom(Parameter p)
        {
            DoubleParameter src = (DoubleParameter)p;
            this.value = src.value;
            this.max = src.max;
            this.min = src.min;
            this.Enabled = src.Enabled;
            this.MultipleValues = src.MultipleValues;
        }
    }

    public class ContinuousDoubleParameter : Parameter
    {
        private double value = 0;

        public static readonly ContinuousDoubleParameter multipleValueParameter = new ContinuousDoubleParameter() { MultipleValues = true };

        public double Value
        {
            get { return value; }
            set { SetProperty(ref this.value, value); }
        }

        public ContinuousDoubleParameter()
        {

        }

        public ContinuousDoubleParameter(double value)
        {
            this.value = value;
        }

        public ContinuousDoubleParameter(ContinuousDoubleParameter src) : base(src)
        {
            this.value = src.value;
        }

        public static ContinuousDoubleParameter operator +(ContinuousDoubleParameter a, ContinuousDoubleParameter b)
        {
            return new ContinuousDoubleParameter { value = a.value + b.value };
        }
        public static ContinuousDoubleParameter operator +(ContinuousDoubleParameter a, double b)
        {
            return new ContinuousDoubleParameter { value = a.value + b };
        }
        public static ContinuousDoubleParameter operator *(ContinuousDoubleParameter a, ContinuousDoubleParameter b)
        {
            return new ContinuousDoubleParameter { value = a.value * b.value };
        }
        public static ContinuousDoubleParameter operator *(ContinuousDoubleParameter a, double b)
        {
            return new ContinuousDoubleParameter { value = a.value * b };
        }
        public static implicit operator double(ContinuousDoubleParameter a)
        {
            return a?.value ?? 0;
        }
        public static implicit operator ContinuousDoubleParameter(double a)
        {
            return new ContinuousDoubleParameter(a);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode() + Enabled.GetHashCode();
        }

        public override object Clone()
        {
            return new ContinuousDoubleParameter(this);
        }

        public override void CopyValueFrom(Parameter p)
        {
            ContinuousDoubleParameter src = (ContinuousDoubleParameter)p;
            this.value = src.value;
            this.Enabled = src.Enabled;
            this.MultipleValues = src.MultipleValues;
        }
    }

    public class IntParameter : Parameter
    {
        private int value = 0;
        private int min = int.MinValue;
        private int max = int.MaxValue;

        public static readonly IntParameter multipleValueParameter = new IntParameter() { MultipleValues = true };

        public int Value
        {
            get { return value; }
            set { SetProperty(ref this.value, value); }
        }
        public int Min
        {
            get { return min; }
            set { SetProperty(ref min, value); }//this.min = value; }
        }
        public int Max
        {
            get { return max; }
            set { SetProperty(ref max, value); }
        }

        public IntParameter()
        {

        }

        public IntParameter(int value)
        {
            this.Value = value;
        }

        public IntParameter(int value, int min, int max)
        {
            this.Value = value;
            this.Max = max;
            this.Min = min;
        }

        public IntParameter(IntParameter src) : base(src)
        {
            this.value = src.value;
            this.max = src.max;
            this.min = src.min;
        }

        public static IntParameter operator +(IntParameter a, IntParameter b)
        {
            return new IntParameter { value = a.value + b.value, min = Math.Max(a.min, b.min), max = Math.Max(a.max, b.max) };
        }
        public static IntParameter operator +(IntParameter a, int b)
        {
            return new IntParameter { value = a.value + b, min = a.min, max = a.max };
        }
        public static IntParameter operator *(IntParameter a, IntParameter b)
        {
            return new IntParameter { value = a.value * b.value, min = Math.Max(a.min, b.min), max = Math.Max(a.max, b.max) };
        }
        public static IntParameter operator *(IntParameter a, int b)
        {
            return new IntParameter { value = a.value * b, min = a.min, max = a.max };
        }
        public static implicit operator int(IntParameter a)
        {
            return a?.value ?? 0;
        }
        public static implicit operator IntParameter(int a)
        {
            return new IntParameter(a);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode() + Enabled.GetHashCode();
        }

        public override object Clone()
        {
            return new IntParameter(this);
        }

        public override void CopyValueFrom(Parameter p)
        {
            IntParameter src = (IntParameter)p;
            this.value = src.value;
            this.max = src.max;
            this.min = src.min;
            this.Enabled = src.Enabled;
            this.MultipleValues = src.MultipleValues;
        }
    }

    public class BoolParameter : Parameter
    {
        private bool value;

        public static readonly BoolParameter multipleValueParameter = new BoolParameter() { MultipleValues = true };

        public bool Value
        {
            get { return value; }
            set { SetProperty(ref this.value, value); }
        }

        public BoolParameter()
        {

        }

        public BoolParameter(bool value)
        {
            this.value = value;
        }

        public BoolParameter(BoolParameter src) : base(src)
        {
            this.value = src.value;
        }

        public static implicit operator BoolParameter(bool v)
        {
            return new BoolParameter(v);
        }
        public static implicit operator bool(BoolParameter v)
        {
            return v?.value ?? false;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode() + Enabled.GetHashCode();
        }

        public override object Clone()
        {
            return new BoolParameter(this);
        }

        public override void CopyValueFrom(Parameter p)
        {
            BoolParameter src = (BoolParameter)p;
            this.value = src.value;
            this.Enabled = src.Enabled;
            this.MultipleValues = src.MultipleValues;
        }
    }

    public class StringParameter : Parameter
    {
        private string value;

        public static readonly StringParameter multipleValueParameter = new StringParameter() { MultipleValues = true };

        public string Value
        {
            get { return value; }
            set { SetProperty(ref this.value, value); }
        }

        public StringParameter()
        {

        }

        public StringParameter(string value)
        {
            Value = value;
        }

        public StringParameter(StringParameter src) : base(src)
        {
            this.value = src.value;
        }

        public static implicit operator StringParameter(string v)
        {
            return new StringParameter(v);
        }
        public static implicit operator string(StringParameter v)
        {
            return v?.value ?? string.Empty;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode() + Enabled.GetHashCode();
        }

        public override object Clone()
        {
            return new StringParameter(this);
        }

        public override void CopyValueFrom(Parameter p)
        {
            StringParameter src = (StringParameter)p;
            this.value = src.value;
            this.Enabled = src.Enabled;
            this.MultipleValues = src.MultipleValues;
        }
    }

    public class ColorParameter : Parameter
    {
        private Color value;

        public static readonly ColorParameter multipleValueParameter = new ColorParameter() { MultipleValues = true };

        public Color Value
        {
            get { return value; }
            set { SetProperty(ref this.value, value); }
        }

        public ColorParameter()
        {

        }

        public ColorParameter(Color value)
        {
            Value = value;
        }

        public ColorParameter(ColorParameter src) : base(src)
        {
            this.value = src.value;
        }

        public static implicit operator ColorParameter(Color v)
        {
            return new ColorParameter(v);
        }
        public static implicit operator Color(ColorParameter v)
        {
            return v?.value ?? Colors.Pink;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode() + Enabled.GetHashCode();
        }

        public override object Clone()
        {
            return new ColorParameter(this);
        }

        public override void CopyValueFrom(Parameter p)
        {
            ColorParameter src = (ColorParameter)p;
            this.value = src.value;
            this.Enabled = src.Enabled;
            this.MultipleValues = src.MultipleValues;
        }
    }

    public class EnumParameter<T> : Parameter
    {
        public T value;
        public Type Type { get { return typeof(T); } }

        public static readonly EnumParameter<T> multipleValueParameter = new EnumParameter<T>() { MultipleValues = true };

        public EnumParameter()
        {

        }

        //Magic...
        public EnumParameter(T value)
        {
            this.value = value;
        }

        public EnumParameter(EnumParameter<T> src) : base(src)
        {
            this.value = src.value;
        }

        public static implicit operator EnumParameter<T>(T v)
        {
            return new EnumParameter<T>(v);
        }
        public static implicit operator T(EnumParameter<T> v)
        {
            return v.value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode() + Enabled.GetHashCode();
        }

        public override object Clone()
        {
            return new EnumParameter<T>(this);
        }

        public override void CopyValueFrom(Parameter p)
        {
            EnumParameter<T> src = (EnumParameter<T>)p;
            this.value = src.value;
            this.Enabled = src.Enabled;
            this.MultipleValues = src.MultipleValues;
        }
    }
}
