using Valve.VR;

public class SteamInputModule : VRInputModule
{

    public SteamVR_Input_Sources m_Source;
    public SteamVR_Action_Boolean m_Click;
    

    
    public override void Process()
    {
        base.Process();

        
        if (m_Click.GetStateDown(m_Source))
            Press();

        
        if (m_Click.GetStateUp(m_Source))
            Release();
    }
    
}
