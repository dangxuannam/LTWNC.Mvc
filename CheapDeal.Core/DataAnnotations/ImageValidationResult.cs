using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheapDeal.Core.DataAnnotations
{
        public enum ImageValidationResult
        {
            // Kiểu nội dung tập tin không phải hình ảnh 
            InvalidMineType,

            //Định dạng ảnh  không được phép upload
            NotAllowedType,

            //Tập tin không phải tập tin ảnh 
            InvalidHeader,

            //Ảnh có kích thước vượt quá quy định 
            OverSize,

            //Hợp lệ 
            Valid,
        InvalidMimeType
    }
    }

