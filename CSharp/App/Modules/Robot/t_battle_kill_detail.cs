//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Modules.Robot
{
    using System;
    using System.Collections.Generic;
    
    public partial class t_battle_kill_detail
    {
        public decimal kill_id { get; set; }
        public decimal character_guid { get; set; }
        public decimal battle_guid { get; set; }
        public long kill_type { get; set; }
        public string kill_name { get; set; }
        public Nullable<decimal> kill_guid { get; set; }
        public string assist_name { get; set; }
        public Nullable<int> kill_time { get; set; }
        public string killed_name { get; set; }
        public System.DateTime create_time { get; set; }
        public System.DateTime modify_time { get; set; }
    }
}
