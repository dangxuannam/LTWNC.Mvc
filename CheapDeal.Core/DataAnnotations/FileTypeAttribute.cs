using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.IO;

namespace CheapDeal.Core.DataAnnotations
{
    ///<summary>
    ///Thuộc tính quy định loại tập tin được phép upload 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FileTypeAttribute : ValidationAttribute
    {
        private readonly List<string> allowFileTypes;
        //allowTypes là chuỗi chứa danh sách mở rộng 
        // của tập tin được phép upload, phân tách nhau bởi dấu phẩy 
        //Sử dụng {FILE_TYPES} trong ErrorMessage để đánh dấu 
        //chỗ sẽ thay thế bởi danh sách các loại file được upload 
        public FileTypeAttribute(string allowTypes)
         : base("Chỉ được phép uploads các tập tin {FILE_TYPES}")
        {
            //phân tích 
            allowFileTypes = allowTypes
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToLower())
                .ToList();
        }

        public override bool IsValid(object value)
        {
            //Lấy đối tượng lưu tập tin được Upload 
            var upload = value as HttpPostedFileBase;

            //Nếu không có tập tin được post lên thì được xem là hợp lệ 
            if (upload == null) return true;

            //Nếu có, kiểm tra loại tập tin có hợp lệ 
            //Lấy phần mở rộng của tập tin 
            var fileExt = Path.GetExtension(upload.FileName);

            if (!string.IsNullOrWhiteSpace(fileExt))
            {
                // bỏ dấu chấm 
                fileExt = fileExt.Substring(1);

                //Trả về true nếu phần mở rộng nằm trong danh sách cho phép 
                return allowFileTypes.Contains(fileExt,
                        StringComparer.OrdinalIgnoreCase);
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            // Nối các định dạng lại thành 1 chuỗi 
            var fileTypes = string.Join(",", allowFileTypes);

            var errorMessage = base.ErrorMessageString;

            //Thay thế {FILES_TYPE} thành các chuỗi định dạng 
            if (errorMessage != null && errorMessage.Contains("{FILE_TYPES}"))
                errorMessage = errorMessage.Replace("{FILE_TYPES}", fileTypes);

            return errorMessage;
        }
    }


}


