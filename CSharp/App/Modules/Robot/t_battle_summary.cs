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
    
    public partial class t_battle_summary
    {
        public decimal character_guid { get; set; }
        public decimal battle_guid { get; set; }
        public Nullable<bool> pvp_or_pve { get; set; }
        public Nullable<bool> is_win { get; set; }
        public Nullable<bool> is_mvp { get; set; }
        public Nullable<bool> is_most_kill_person { get; set; }
        public Nullable<bool> is_most_assist { get; set; }
        public Nullable<bool> is_most_money { get; set; }
        public Nullable<bool> is_down_tower { get; set; }
        public Nullable<bool> is_most_kill_friend { get; set; }
        public Nullable<bool> is_lost_mvp { get; set; }
        public Nullable<bool> is_s_pve_map { get; set; }
        public Nullable<bool> is_first_blood { get; set; }
        public Nullable<bool> is_pve_most_kill_boss { get; set; }
        public Nullable<bool> is_pve_most_take_damage { get; set; }
        public Nullable<bool> is_pve_most_healing { get; set; }
        public Nullable<bool> is_pve_most_damage { get; set; }
        public string map_type { get; set; }
        public string battle_date { get; set; }
        public Nullable<int> battle_time { get; set; }
        public Nullable<bool> is_escape { get; set; }
        public Nullable<int> hero_id { get; set; }
        public Nullable<int> match_type { get; set; }
        public Nullable<bool> is_reinforce { get; set; }
        public Nullable<int> pvp_kill_count { get; set; }
        public Nullable<int> pvp_assist_count { get; set; }
        public Nullable<int> pvp_dead_count { get; set; }
        public Nullable<int> pvp_most_continuous_kill_number { get; set; }
        public Nullable<int> pvp_score { get; set; }
        public Nullable<int> pve_boss_kill { get; set; }
        public Nullable<int> pve_kill_hostile_soldier { get; set; }
        public Nullable<int> pve_total_money { get; set; }
        public Nullable<int> pve_get_level { get; set; }
        public Nullable<bool> is_employ_battle { get; set; }
        public System.DateTime create_time { get; set; }
        public System.DateTime modify_time { get; set; }
    }
}
