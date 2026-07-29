using Dalamud.Configuration;

namespace Doorbell; 

public class Config : IPluginConfiguration {
    public int Version { get; set; } = 1;
    
    public Alert Entered { get; set; } = new() {
        ChatEnabled = true,
        SoundEnabled = true,
        ChatFormat = "<link> 進入了房屋。",
        SoundFile = ""
    };
    
    public Alert Left { get; set; } = new() {
        ChatEnabled = false,
        SoundEnabled = false,
        ChatFormat = "<link> 離開了房屋。"
    };
    
    public Alert AlreadyHere { get; set; } = new() {
        ChatEnabled = false,
        SoundEnabled = false,
        ChatFormat = "你抵達時，<link> 已經在房屋內。"
    };
}
