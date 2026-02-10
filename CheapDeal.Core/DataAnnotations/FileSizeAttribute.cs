using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CheapDeal.Core.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FileSizeAttribute : ValidationAttribute 
    {
        private readonly int maxSize;

        // maxSize = Dung lượng tối đa,tính theo Megabytes 
        // Sử dụng {MAXSIZE} để đánh dấu sẽ thay bằng giá trị maxSize 
        public FileSizeAttribute (int maxSize)
            : base("Dung lượng tập tin không được quá {maxSize} MB.")
        {
            this.maxSize = maxSize;
        }

        public override bool IsValid (object  value)
        {
            //Lấy đối tượng tập tin được upload 
            var upload = value as HttpPostedFileBase;

            //Nếu không có tập tin được post lên thì xem như hợp lệ 
            if (upload == null) return true;

            //Nếu có, kiểm tra dung lượng có hợp lệ 
            //Vì ContentLength tính theo byte nên phải nhân 

            //maxSize với 1024 * 1024 để ra số bytes.
            return upload.ContentLength <= maxSize * 1024 * 1024;
        }
        public override string FormatErrorMessage(string name)
        {
            var errorMessage = base.ErrorMessageString;

            if (errorMessage != null && errorMessage.Contains("{MAXSIZE}"))
                errorMessage = errorMessage.Replace(
                     "{MAXSIZE}", maxSize.ToString());

            return errorMessage;
        }
    }
}
