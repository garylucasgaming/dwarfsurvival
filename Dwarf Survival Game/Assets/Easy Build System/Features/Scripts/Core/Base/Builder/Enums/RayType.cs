namespace EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums
{
    public enum RayType
    {
        FirstPerson,
        TopDown,
        ThirdPerson,
        #if ENABLE_INPUT_SYSTEM
        VirtualRealityPerson
        #endif
    }
}