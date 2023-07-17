namespace System.Web.Mvc.ModelStateChangeSet
{
    public class ModelChange<T>
    {
        public string Property { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}