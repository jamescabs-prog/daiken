using System;
using System.Collections.Generic;

namespace Drafters.ViewModels
{
    public class LineInput
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateRequisitionVM
    {
        public CreateRequisitionVM() { Lines = new List<LineInput>(); }

        public string JobOrderNo { get; set; }
        public string Purpose { get; set; }
        public DateTime DateNeeded { get; set; }
        public List<LineInput> Lines { get; set; }
    }
}
