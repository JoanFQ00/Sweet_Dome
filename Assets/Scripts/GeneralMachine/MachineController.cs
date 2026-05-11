//using UnityEngine;

//public class Machine1Controller : MonoBehaviour
//{
//    public TubeController pinkDispenser;
//    public TubeController magentaDispenser;

//    public bool magentaUnlocked;

//    private TubeController currentDispenser;

//    private void Start()
//    {
//        currentDispenser = pinkDispenser;
//    }

//    public void MoveLeft()
//    {
//        currentDispenser.MoveLeft();
//    }

//    public void MoveRight()
//    {
//        currentDispenser.MoveRight();
//    }

//    public void SwapDispenser()
//    {
//        if (!magentaUnlocked)
//            return;

//        if (currentDispenser == pinkDispenser)
//            currentDispenser = magentaDispenser;
//        else
//            currentDispenser = pinkDispenser;
//    }

//    public void LeverNeutral()
//    {
//        currentDispenser.SetLeverMode(TubeController.LeverMode.Neutral);
//    }

//    public void LeverSuck()
//    {
//        currentDispenser.SetLeverMode(TubeController.LeverMode.Suck);
//    }

//    public void LeverOut()
//    {
//        currentDispenser.SetLeverMode(TubeController.LeverMode.Out);
//    }

//    public void StartDump()
//    {
//        currentDispenser.isDumping = true;
//    }

//    public void StopDump()
//    {
//        currentDispenser.isDumping = false;
//    }
//}