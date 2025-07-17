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
                    new (){Code = 4401, Name = "广州（粤）"     },
                    new (){Code = 0   , Name = "广州（穗）"     },
                    new (){Code = 1   , Name = "广东沿海城市"   },
                    new (){Code = 2   , Name = "广东近岸洋面"   },//地市无关项分配到8
                    new (){Code = 4402, Name = "韶关"           },
                    new (){Code = 4403, Name = "深圳"           },
                    new (){Code = 4404, Name = "珠海"           },
                    new (){Code = 4405, Name = "汕头"           },
                    new (){Code = 4406, Name = "佛山"           },
                    new (){Code = 4407, Name = "江门"           },
                    new (){Code = 4408, Name = "湛江"           },
                    new (){Code = 4409, Name = "茂名"           },
                    new (){Code = 4412, Name = "肇庆"           },
                    new (){Code = 4413, Name = "惠州"           },
                    new (){Code = 4414, Name = "梅州"           },
                    new (){Code = 4415, Name = "汕尾"           },
                    new (){Code = 4417, Name = "阳江"           },
                    new (){Code = 4416, Name = "河源"           },
                    new (){Code = 4418, Name = "清远"           },
                    new (){Code = 4419, Name = "东莞"           },
                    new (){Code = 4420, Name = "中山"           },
                    new (){Code = 4451, Name = "潮州"           },
                    new (){Code = 4452, Name = "揭阳"           },
                    new (){Code = 4453, Name = "云浮"           },
                    new (){Code = 4505, Name = "北海"           },
                    new (){Code = 4509, Name = "玉林"           },
                    new (){Code = 44  , Name = "广东省"         }
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
