using DevLib.ModuleSystem;

namespace CIW.Code.System
{
    /// <summary>
    /// 게임 개체의 공통 기반입니다. 모듈 검색과 초기화는 ModuleOwner가 담당하고,
    /// 사망 같은 생명 주기는 각 개체의 전용 모듈이 담당합니다.
    /// </summary>
    public abstract class Entity : ModuleOwner
    {
    }
}
