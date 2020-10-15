namespace FuLu.Passport.Domain.Models
{
    public class ResultBase<T>
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }
    }
}