using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Web;
using System.Drawing;

namespace CheapDeal.Core.DataAnnotations
{
    ///<summary>
    ///Thuộc tính được quy định kích thước tối đa 
    ///hình ảnh được phép upload 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ImageSizeAttribute : ValidationAttribute 
    {
        private ImageValidationResult ivResult = ImageValidationResult.Valid;

        ///<summary>
        ///Mảng lưu các định dạng nội dung hình ảnh 
        /// </summary>
        private static string[] mineTypes =
            {
                "image/jpg", "image/jpeg", "image/pjpeg", "image/gif",
                "image/x-png", "image/png", "image/bmp", "image/x-icon",
                "image/x-tiff", "image/tiff",
        };

        ///<summary>
        /// Mảng lưu các loại hình ảnh được upload 
        /// </summary>
        private static string[] imageExts =
            {
                ".jpg", ".jpeg", ".pjpeg", ".gif", ".x-png", ".png", ".bmp", ".x-icon", ".x-tiff", ".tiff",
        };

        ///<summary>
        /// Chiều rộng tối đa tính theo pixel 
        /// </summary>
        public int Width { get; set; }

        ///<summary>
        /// Chiều cao tối đa tính theo pixel 
        /// </summary>
        public int Height { get; set; }

        //Sử dụng {WIDTH} VÀ {HEIGHT} để đánh dấu vị trí 
        // sẽ thay thế nội dung và chiều cao tối đa
        public ImageSizeAttribute() 
            : base ("Kích thước hình vượt quá cỡ {WIDTH}x{HEIGHT}") { }

        ///<summary>
        /// Kiểm tra kiểu nội dung của tập tin được upload 
        /// có phải là kiểu nội dung của hình ảnh 
        /// </summary>
        /// <param name="uplaod ">Tập tin được upload</param>
        /// <returns> 
        /// Trả về false nếu không đúng định dạng nọi dung 
        /// cả tất cả các loại hình ảnh. True nếu ngược lại 
        /// </returns>
        private bool CheckMineTypes(HttpPostedFileBase upload) 
        { 
            //Lấy kiểu nội dung của tập tin được upload 
            var contentType = upload.ContentType.ToLower();

            //Nếu kiểu nội dung không thuộc về hình ảnh 
            if (!mineTypes.Contains(contentType)) 
            {
                //thì đánh dấu MINE không hợp lý 
                ivResult = ImageValidationResult.InvalidMineType;
                return false;
            }
            return true;
        }

        ///<summary>
        /// Kiểm tra phần tên mở rộng  của tập tin
        /// có nằm trong danh sách những hình ảnh 
        /// được upload không ?
        /// </summary>
        /// <param name="uplaod ">Tập tin được upload</param>
        /// <returns> 
        /// Trả về false nếu không đúng định dạng nọi dung 
        /// cả tất cả các loại hình ảnh. True nếu ngược lại 
        /// </returns>
    
        ///Trả về false nếu tên mở rộng không nằm trong ảnh 
        ///các định dạng được phép upload. True nếu ngược lại 
        ///</returns>
        private bool CheckFileExtension(HttpPostedFileBase upload)
        {
            //Lấy phần mở rộng của tập tin 
            var fileExt = Path.GetExtension(upload.FileName);

            //Nếu có phần tên mở rộng 
            if (!string.IsNullOrWhiteSpace(fileExt)) 
            { 
            // Trả về true nếu phần mở rộng nằm trong danh sách cho phép
             if (imageExts.Contains(fileExt, StringComparer.OrdinalIgnoreCase))
                    return true;
            }

            //Đánh dấu định dạng file không hợp lệ 
            ivResult = ImageValidationResult.NotAllowedType;

            return false;
        }
        ///<summary>
        ///Kiểm tra kích thước của file ảnh được upload có vuợt 
        ///quá khổ quy định bởi 2 thuộc tính Width và Height?
        /// </summary>
        /// <param name="upload">Tập tin ảnh được upload</param>
        /// <returns> Trả về false nếu kích thước ảnh quá lớn </returns>
        private bool CheckImageSize (HttpPostedFileBase upload)
        {
            if (!upload.InputStream.CanRead)
            {
                ivResult = ImageValidationResult.InvalidHeader;
                return false;
            }
            try
            {
                //Tạo hình ảnh từ luồng upload 
                using (var image = Image.FromStream(upload.InputStream))
                {
                    //Kiểm tra kích thước ảnh có vượt quá cỡ cho phép 
                    // Nếu có trả về false 
                    if (image.Width > Width || image.Height > Height)
                    {
                        ivResult = ImageValidationResult.OverSize;
                        return false;
                    }
                }
                return true;
            }
            catch (Exception) 
            {
                ivResult = ImageValidationResult.InvalidHeader;
                return false;
            }
        }
        public override bool IsValid (object value) 
        { 
         //Lấy đối tượng lưu tập tin upload 
         var upload = value as HttpPostedFileBase;

            //Nếu không có tập tin được post lên thì xem thì hợp lệ 
            if (upload == null) return true;

            //Lần lượt thực hiện các thao tác kiểm tra 
            bool valid = true;

            //Kiểm tra mime types 
            valid = CheckMineTypes(upload );

            //Kiểm tra phần đuôi mở rộng 
            if (valid) valid = CheckFileExtension(upload);

            //Kiểm tra header có đúng định dạng ảnh 
            // if(valid) valid = CheckFileHeader(upload);

            //Kiểm tra hình đó có đúng kích cỡ 
            if (valid)  valid = CheckFileExtension(upload);

            return valid ;
        }
        public override string FormatErrorMessage(string Name) 
        {
            var errorMessage = base.ErrorMessageString;

            switch (ivResult) 
            {
                case ImageValidationResult.InvalidHeader:
                    return "Không thể đọc được nội dung ảnh";

             case ImageValidationResult.InvalidMimeType:
             case ImageValidationResult.NotAllowedType:
                    return "Hệ thống không hỗ trợ định dạng này";

                case ImageValidationResult.OverSize:
                    if (errorMessage != null)
                    {
                        if (errorMessage.Contains("{WIDTH}"))
                            errorMessage = errorMessage.Replace(
                                            "{WIDTH}", Width.ToString());

                        if (errorMessage.Contains("{HEIGHT}"))
                            errorMessage = errorMessage.Replace(
                                            "{HEIGHT}", Width.ToString());
                    }
                    return errorMessage;

                default: 
                    return errorMessage;
            }
        }
    }

}
