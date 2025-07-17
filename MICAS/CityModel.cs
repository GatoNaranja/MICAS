using System.Collections.ObjectModel;
using System.Linq;

namespace MICAS
{
    public class CityModel
    {
        public ObservableCollection<Region> regions { get; set; }
        public Region selectedRegion { get; set; }
        public CityModel()
        {
            regions = new ObservableCollection<Region>{
                    new Region(){Code = 4401, Name = "广州（粤）"     },
                    new Region(){Code = 0   , Name = "广州（穗）"     },
                    new Region(){Code = 1   , Name = "广东沿海城市"   },
                    new Region(){Code = 2   , Name = "广东近岸洋面"   },//地市无关项分配到8
                    new Region(){Code = 4402, Name = "韶关"           },
                    new Region(){Code = 4403, Name = "深圳"           },
                    new Region(){Code = 4404, Name = "珠海"           },
                    new Region(){Code = 4405, Name = "汕头"           },
                    new Region(){Code = 4406, Name = "佛山"           },
                    new Region(){Code = 4407, Name = "江门"           },
                    new Region(){Code = 4408, Name = "湛江"           },
                    new Region(){Code = 4409, Name = "茂名"           },
                    new Region(){Code = 4412, Name = "肇庆"           },
                    new Region(){Code = 4413, Name = "惠州"           },
                    new Region(){Code = 4414, Name = "梅州"           },
                    new Region(){Code = 4415, Name = "汕尾"           },
                    new Region(){Code = 4417, Name = "阳江"           },
                    new Region(){Code = 4416, Name = "河源"           },
                    new Region(){Code = 4418, Name = "清远"           },
                    new Region(){Code = 4419, Name = "东莞"           },
                    new Region(){Code = 4420, Name = "中山"           },
                    new Region(){Code = 4451, Name = "潮州"           },
                    new Region(){Code = 4452, Name = "揭阳"           },
                    new Region(){Code = 4453, Name = "云浮"           },
                    new Region(){Code = 4505, Name = "北海"           },
                    new Region(){Code = 4509, Name = "玉林"           },
                    new Region(){Code = 44  , Name = "广东省"         }
                };
            selectedRegion = regions.FirstOrDefault(f => f.Code == 4401);
        }
    }

    public class Region
    {
        public int Code { get; set; }
        public string Name { get; set; }
    }
}
