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
    
    public partial class t_social_group_basic
    {
        public decimal group_guid { get; set; }
        public string group_name { get; set; }
        public int group_type { get; set; }
        public decimal owner_guid { get; set; }
        public string owner_name { get; set; }
        public int member_size { get; set; }
        public int manager_size { get; set; }
        public string bulletin { get; set; }
        public int join_rule { get; set; }
        public System.DateTime modify_time { get; set; }
        public System.DateTime create_time { get; set; }
    }
}
