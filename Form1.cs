using MvCamCtrl.NET;
using MvCamCtrl.NET.CameraParams;

using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.Flann;
using OpenCvSharp.Extensions;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;

namespace A_Scout_D
{
    public enum STATE_VAL
    {
        CAM_IDLE = 0,
        CAM_OPENED,
        CAM_PREVIEW,
        CAM_SAVE,
        CAM_SAVE_DONE,
        CAM_PLAY,
        CAM_PLAY_PAUSE        
    }

    public enum ErrorCode
    {
        NoError = 0x0000,
        NoCamera,
        Camera1CreateFail,
        Camera2CreateFail,
        Camera1HandleFail,
        Camera2HandleFail,
        Camera1OpenFail,
        Camera2OpenFail,

        Unknown        
    }

    public partial class Form1 : Form
    {
        int m_nValidCamNum = 0;
        private CCamera m_MyCamera1 = null;
        private CCamera m_MyCamera2 = null;
        List<CCameraInfo> m_ltDeviceList = new List<CCameraInfo>();

        bool m_bCam1Thread = false;
        Thread m_hCam1ReceiveThread = null;
        bool m_bCam2Thread = false;
        Thread m_hCam2ReceiveThread = null;

        int m_Cam1Index = 0;
        int m_Cam2Index = 1;

        STATE_VAL m_Cam1State = STATE_VAL.CAM_IDLE;
        STATE_VAL m_Cam2State = STATE_VAL.CAM_IDLE;

        MV_FRAME_OUT_INFO_EX m_stCam1ImageInfo;
        MV_FRAME_OUT_INFO_EX m_stCam2ImageInfo;

        byte[] m_pCam1DisplayBuffer = null;
        byte[] m_pCam2DisplayBuffer = null;
        byte[][] m_pSaveBuffer1 = new byte[Constants.CAM1_SAVE_COUNT][];
        byte[][] m_pSaveBuffer2 = new byte[Constants.CAM2_SAVE_COUNT][];

        bool m_bCam1NewFrame = false;
        bool m_bCam2NewFrame = false;

        int m_nSaveIndex1 = 0;
        int m_nSaveIndex2 = 0;

        int m_Cam1SaveCount = Constants.CAM1_SAVE_COUNT;
        int m_Cam2SaveCount = Constants.CAM2_SAVE_COUNT;

        bool m_bCaptureFlag1 = false;
        bool m_bCaptureFlag2 = false;

        Mat m_pOriginalImage1 = null;
        Mat m_pDisplayImage1 = null;
        Mat m_pDisplayMode1 = null;

        Mat m_pOriginalImage2 = null;
        Mat m_pDisplayImage2 = null;
        Mat m_pDisplayMode2 = null;

        int m_LastDisplayIndex1 = 0;
        int m_LastDisplayIndex2 = 0;
                
        int m_nCam1Interval = 1;
        int m_nCam2Interval = 1;

        double m_nFrameRate1 = 0.0;
        double m_nFrameRate2 = 0.0;
        int m_nSaveFrameRate = 120;        

        bool m_bFocusMode = false;

        int m_FocusValue1 = 0;
        int m_FocusValue2 = 0;

        Stopwatch FPSWatch = new Stopwatch();
        long m_nFrameCount1 = 0;
        long m_nFrameCount2 = 0;

        static System.Windows.Forms.Timer GrabInfoTimer;
        static System.Windows.Forms.Timer PlayProgressTimer;

        private SaveFileDialog saveFileDialog1 = null;

        private static cbOutputExdelegate Cam1Callback;
        static void Cam1CallbackFunc(IntPtr pData, ref MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            if (pUser != null)
            {
                Form1 thisClass = (Form1)GCHandle.FromIntPtr(pUser).Target;
                thisClass.m_nFrameCount1++;
                switch (thisClass.m_Cam1State)
                {
                    case STATE_VAL.CAM_PREVIEW:
                        if (thisClass.m_bCam1NewFrame == false)
                        {
                            Marshal.Copy(pData, thisClass.m_pCam1DisplayBuffer, 0, (int)pFrameInfo.nFrameLen);
                            thisClass.m_stCam1ImageInfo = pFrameInfo;
                            thisClass.m_bCam1NewFrame = true;
                        }
                        break;

                    case STATE_VAL.CAM_SAVE:
                        Marshal.Copy(pData, thisClass.m_pSaveBuffer1[thisClass.m_nSaveIndex1], 0, (int)pFrameInfo.nFrameLen);
                        thisClass.m_stCam1ImageInfo = pFrameInfo;
                        thisClass.m_bCam1NewFrame = true;
                        thisClass.m_nSaveIndex1++;
                        if (thisClass.m_nSaveIndex1 == thisClass.m_Cam1SaveCount)
                        {
                            thisClass.m_nSaveIndex1 = 0;
                            thisClass.m_bCaptureFlag1 = true;
                            thisClass.m_Cam1State = STATE_VAL.CAM_SAVE_DONE;
                        }
                        break;

                    default:

                        break;
                }
            }
        }

        private static cbOutputExdelegate Cam2Callback;
        static void Cam2CallbackFunc(IntPtr pData, ref MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            if (pUser != null)
            {
                Form1 thisClass = (Form1)GCHandle.FromIntPtr(pUser).Target;
                thisClass.m_nFrameCount2++;
                switch (thisClass.m_Cam2State)
                {
                    case STATE_VAL.CAM_PREVIEW:
                        Marshal.Copy(pData, thisClass.m_pCam2DisplayBuffer, 0, (int)pFrameInfo.nFrameLen);
                        thisClass.m_stCam2ImageInfo = pFrameInfo;
                        thisClass.m_bCam2NewFrame = true;
                        break;

                    case STATE_VAL.CAM_SAVE:
                        Marshal.Copy(pData, thisClass.m_pSaveBuffer2[thisClass.m_nSaveIndex2], 0, (int)pFrameInfo.nFrameLen);
                        thisClass.m_stCam2ImageInfo = pFrameInfo;
                        thisClass.m_bCam2NewFrame = true;
                        thisClass.m_nSaveIndex2++;
                        if (thisClass.m_nSaveIndex2 == thisClass.m_Cam2SaveCount)
                        {
                            thisClass.m_nSaveIndex2 = 0;
                            thisClass.m_bCaptureFlag2 = true;
                            thisClass.m_Cam2State = STATE_VAL.CAM_SAVE_DONE;
                        }
                        break;

                    default:

                        break;
                }
            }
        }


        public Form1()
        {
            InitializeComponent();
            DeviceListAcq();
            if (m_nValidCamNum > 0)
            {
                ThreadCallbackStart();
                MemoryInitialize();
            }
            InitializeContents();
        }

        private ErrorCode DeviceListAcq()
        {
            ErrorCode AScoutErr, AScoutErr1;
            System.GC.Collect();
            m_ltDeviceList.Clear();
            CUSBCameraInfo usbInfo1, usbInfo2;

            AScoutErr = ErrorCode.NoError;

            int nRet = CSystem.EnumDevices(CSystem.MV_USB_DEVICE, ref m_ltDeviceList);
            if (0 != nRet)
            {
                AScoutErr = ErrorCode.NoCamera;
                return AScoutErr;
            }
            lbState.Text = "Camera State : " + "Connection Check " + "Connect Camera && " + "Restart";
            m_nValidCamNum = 0;
            if(m_ltDeviceList.Count == 0)
            {
                m_nValidCamNum = 0;
                AScoutErr = ErrorCode.NoCamera;
            }
            else if( m_ltDeviceList.Count == 1)
            {
                if (m_ltDeviceList[0].nTLayerType == CSystem.MV_USB_DEVICE)
                {
                    CUSBCameraInfo usbInfo = (CUSBCameraInfo)m_ltDeviceList[0];
                    m_nValidCamNum = 1;
                    AScoutErr = CameraOpen(0, 0);                    
                }
            }
            else if ((m_ltDeviceList.Count == 2)&& (m_ltDeviceList[0].nTLayerType == CSystem.MV_USB_DEVICE)&& (m_ltDeviceList[1].nTLayerType == CSystem.MV_USB_DEVICE))
            {
                m_nValidCamNum = 2;
                usbInfo1 = (CUSBCameraInfo)m_ltDeviceList[0];
                usbInfo2 = (CUSBCameraInfo)m_ltDeviceList[1];
                string usb1UDName;
                string usb2UDName;
                usb1UDName = Regex.Replace(usbInfo1.UserDefinedName, @"[^A-Z0-9 -]", string.Empty);
                usb2UDName = Regex.Replace(usbInfo2.UserDefinedName, @"[^A-Z0-9 -]", string.Empty);

                // 둘 다 Define 되었는데 중복되는 경우
                if ((usb1UDName == (string)"FON")&& (usb2UDName == (string)"FON"))
                {
                    AScoutErr = CameraOpen(0, 0);
                    AScoutErr1 = CameraOpen(1, 1);
                    m_Cam1Index = 0;
                    m_Cam2Index = 1;
                }
                else if((usb1UDName == (string)"DTL")&& (usb2UDName == (string)"DTL"))
                {
                    AScoutErr = CameraOpen(0, 0);
                    AScoutErr1 = CameraOpen(1, 1);
                    m_Cam1Index = 0;
                    m_Cam2Index = 1;
                }
                // 둘 다 제대로 Define 되어 있는 경우
                else if ((usb1UDName == (string)"FON")&& (usb2UDName == (string)"DTL"))
                {
                    AScoutErr = CameraOpen(0, 0);
                    AScoutErr1 = CameraOpen(1, 1);
                    m_Cam1Index = 0;
                    m_Cam2Index = 1;
                }
                else if ((usb1UDName == (string)"DTL") && (usb2UDName == (string)"FON"))
                {
                    AScoutErr = CameraOpen(0, 1);
                    AScoutErr1 = CameraOpen(1, 0);
                    m_Cam1Index = 1;
                    m_Cam2Index = 0;
                }
                // 둘 중 하나만 제대로 Define 된 경우
                else if (usb1UDName == (string)"FON") 
                {
                    AScoutErr = CameraOpen(0, 0);
                    AScoutErr1 = CameraOpen(1, 1);
                    m_Cam1Index = 0;
                    m_Cam2Index = 1;
                }
                else if (usb1UDName == (string)"DTL")
                {
                    AScoutErr = CameraOpen(0, 1);
                    AScoutErr1 = CameraOpen(1, 0);
                    m_Cam1Index = 1;
                    m_Cam2Index = 0;
                }
                else if (usb2UDName == (string)"FON")
                {
                    AScoutErr = CameraOpen(0, 1);
                    AScoutErr1 = CameraOpen(1, 0);
                    m_Cam1Index = 1;
                    m_Cam2Index = 0;
                }
                else if (usb2UDName == (string)"DTL")
                {
                    AScoutErr = CameraOpen(0, 0);
                    AScoutErr1 = CameraOpen(1, 1);
                    m_Cam1Index = 0;
                    m_Cam2Index = 1;
                }
                // 둘 다 제대로 define 되어 있지 않은 경우
                else
                {
                    AScoutErr = CameraOpen(0, 0);
                    AScoutErr1 = CameraOpen(1, 1);
                    m_Cam1Index = 0;
                    m_Cam2Index = 1;
                }
                if((AScoutErr == ErrorCode.NoError)&&(AScoutErr1 == ErrorCode.NoError))
                {
                    return AScoutErr;
                }
                else if((AScoutErr == ErrorCode.NoError)&& (AScoutErr1 != ErrorCode.NoError))
                {
                    return AScoutErr1;
                }
                else if ((AScoutErr != ErrorCode.NoError) && (AScoutErr1 == ErrorCode.NoError))
                {
                    return AScoutErr;
                }
            }
            return AScoutErr;
        }

        private ErrorCode CameraOpen(int Index, int CamNumber)
        {
            ErrorCode AScoutErr = ErrorCode.NoError;
            if (CamNumber == 0)
            {
                if (null == m_MyCamera1)
                {
                    m_MyCamera1 = new CCamera();
                    if (null == m_MyCamera1)
                    {
                        AScoutErr = ErrorCode.Camera1CreateFail;
                        return AScoutErr;
                    }
                }

                CCameraInfo device = m_ltDeviceList[Index];
                int nRet = m_MyCamera1.CreateHandle(ref device);
                if (CErrorDefine.MV_OK != nRet)
                {
                    AScoutErr = ErrorCode.Camera1HandleFail;
                    return AScoutErr;
                }

                nRet = m_MyCamera1.OpenDevice();
                if (CErrorDefine.MV_OK != nRet)
                {
                    m_MyCamera1.DestroyHandle();
                    AScoutErr = ErrorCode.Camera1OpenFail;
                    return AScoutErr; 
                }

                float DigitalShift = 5.0f;
                m_MyCamera1.SetEnumValue("AcquisitionMode", (uint)MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                m_MyCamera1.SetEnumValue("TriggerMode", (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                m_MyCamera1.SetIntValue("Width", Constants.CAM1_WIDTH);
                m_MyCamera1.SetBoolValue("DigitalShiftEnable", true);
                m_MyCamera1.SetFloatValue("DigitalShift", DigitalShift);
                m_Cam1State = STATE_VAL.CAM_OPENED;
                lbState.Text = "Camera State : " + "Camera Ready";
            }
            else if (CamNumber == 1)
            {
                if (null == m_MyCamera2)
                {
                    m_MyCamera2 = new CCamera();
                    if (null == m_MyCamera2)
                    {
                        AScoutErr = ErrorCode.Camera1CreateFail;
                        return AScoutErr;
                    }
                }

                CCameraInfo device = m_ltDeviceList[Index];
                int nRet = m_MyCamera2.CreateHandle(ref device);
                if (CErrorDefine.MV_OK != nRet)
                {
                    AScoutErr = ErrorCode.Camera2HandleFail;
                    return AScoutErr;
                }

                nRet = m_MyCamera2.OpenDevice();
                if (CErrorDefine.MV_OK != nRet)
                {
                    m_MyCamera2.DestroyHandle();
                    AScoutErr = ErrorCode.Camera2OpenFail;
                    return AScoutErr;
                }
                float DigitalShift = 5.0f;
                m_MyCamera2.SetEnumValue("AcquisitionMode", (uint)MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                m_MyCamera2.SetEnumValue("TriggerMode", (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                m_MyCamera2.SetIntValue("Width", Constants.CAM2_WIDTH);
                m_MyCamera2.SetBoolValue("DigitalShiftEnable", true);
                m_MyCamera2.SetFloatValue("DigitalShift", DigitalShift);
                m_Cam2State = STATE_VAL.CAM_OPENED;
                lbState.Text = "Camera State : " + "Camera Ready";
            }
            return AScoutErr;
        }

        private void ThreadCallbackStart()
        {
            int nRet;
            // this 객체를 GCHandle로 래핑합니다.
            GCHandle handle = GCHandle.Alloc(this);
            // GCHandle을 IntPtr로 변환합니다.
            IntPtr thisClassPtr = GCHandle.ToIntPtr(handle);

            if (m_MyCamera1 != null)
            {
                m_bCam1Thread = true;
                m_hCam1ReceiveThread = new Thread(Cam1ThreadProcess);
                m_hCam1ReceiveThread.Start();

                Cam1Callback = new cbOutputExdelegate(Cam1CallbackFunc);
                nRet = m_MyCamera1.RegisterImageCallBackEx(Cam1Callback, thisClassPtr);
                if (CErrorDefine.MV_OK != nRet)
                {

                }
            }

            if (m_MyCamera2 != null)
            {
                m_bCam2Thread = true;
                m_hCam2ReceiveThread = new Thread(Cam2ThreadProcess);
                m_hCam2ReceiveThread.Start();

                Cam2Callback = new cbOutputExdelegate(Cam2CallbackFunc);
                nRet = m_MyCamera2.RegisterImageCallBackEx(Cam2Callback, thisClassPtr);
                if (CErrorDefine.MV_OK != nRet)
                {

                }
            }
        }

        public void Cam1ThreadProcess()
        {
            while (m_bCam1Thread)
            {
                switch (m_Cam1State)
                {
                    case STATE_VAL.CAM_PREVIEW:
                        if ((m_bCam1NewFrame) && (m_pOriginalImage1 == null) && (m_pDisplayImage1 == null))
                        {
                            CreateImageBuffer1();
                        }
                        else
                        {
                            if ((m_bCam1NewFrame) && (m_pOriginalImage1 != null) && (m_pDisplayImage1 != null))
                            {
                                ImageDisplayCam1();
                                m_bCam1NewFrame = false;
                            }
                        }
                        Thread.Sleep(30);
                        break;
                    case STATE_VAL.CAM_SAVE:
                        if ((m_bCam1NewFrame) && (m_nSaveIndex1 > 1))
                        {
                            m_bCam1NewFrame = false;
                            Buffer.BlockCopy(m_pSaveBuffer1[m_nSaveIndex1 - 1], 0, m_pCam1DisplayBuffer, 0, (int)m_stCam1ImageInfo.nFrameLen);
                            ImageDisplayCam1();
                            Thread.Sleep(30);
                        }
                        break;
                    case STATE_VAL.CAM_SAVE_DONE:
                        if ((m_Cam2State == STATE_VAL.CAM_IDLE) || (m_Cam2State == STATE_VAL.CAM_SAVE_DONE))
                        {
                            GrabInfoTimer.Stop();

                            if (m_MyCamera1 != null)
                            {
                                m_MyCamera1.StopGrabbing();
                                m_Cam1State = STATE_VAL.CAM_OPENED;
                                lbState.Invoke(new Action(() =>
                                {
                                    lbState.Text = "Camera State : Recording complete";
                                }));

                            }

                            if (m_MyCamera2 != null)
                            {
                                m_MyCamera2.StopGrabbing();
                                m_Cam2State = STATE_VAL.CAM_OPENED;
                                lbState.Invoke(new Action(() =>
                                {
                                    lbState.Text = "Camera State : Recording complete";
                                }));
                            }

                        }

                        break;
                    case STATE_VAL.CAM_PLAY:
                        if ((m_pOriginalImage1 == null) && (m_pDisplayImage1 == null))
                        {
                            CreateImageBuffer1();
                        }
                        else
                        {
                            if (m_LastDisplayIndex1 != m_nSaveIndex1)
                            {
                                Buffer.BlockCopy(m_pSaveBuffer1[m_nSaveIndex1], 0, m_pCam1DisplayBuffer, 0, (int)m_stCam1ImageInfo.nFrameLen);
                                ImageDisplayCam1();
                                m_LastDisplayIndex1 = m_nSaveIndex1;
                            }
                        }

                        m_nSaveIndex1++;
                        if (m_nSaveIndex1 == m_Cam1SaveCount)
                        {
                            m_nSaveIndex1--;
                            Thread.Sleep(100);
                            if (m_MyCamera1 != null)
                            {
                                m_Cam1State = STATE_VAL.CAM_OPENED;
                            }
                            else
                            {
                                m_Cam1State = STATE_VAL.CAM_IDLE;
                            }
                            
                            m_nSaveIndex1 = 0;
                            Thread.Sleep(300);
                            PlayProgressTimer.Stop();
                            lbState.Invoke(new Action(() =>
                            {
                                lbState.Text = "Camera State : Play End";
                            }));                           
                        }
                        Thread.Sleep(m_nCam1Interval);
                        break;
                    case STATE_VAL.CAM_PLAY_PAUSE:

                        Buffer.BlockCopy(m_pSaveBuffer1[m_nSaveIndex1], 0, m_pCam1DisplayBuffer, 0, (int)m_stCam1ImageInfo.nFrameLen);
                        ImageDisplayCam1();

                        if (m_MyCamera1 != null)
                        {
                            m_Cam1State = STATE_VAL.CAM_OPENED;
                        }
                        else
                        {
                            m_Cam1State = STATE_VAL.CAM_IDLE;
                        }

                        break;

                    default:
                        Thread.Sleep(10);
                        break;
                }
                Thread.Sleep(1);
            }
        }

        public void Cam2ThreadProcess()
        {
            while (m_bCam2Thread)
            {
                switch (m_Cam2State)
                {
                    case STATE_VAL.CAM_PREVIEW:
                        if ((m_bCam2NewFrame) && (m_pOriginalImage2 == null) && (m_pDisplayImage2 == null))
                        {
                            CreateImageBuffer2();
                        }
                        else
                        {
                            if ((m_bCam2NewFrame) && (m_pOriginalImage2 != null) && (m_pDisplayImage2 != null))
                            {
                                ImageDisplayCam2();
                                m_bCam2NewFrame = false;
                            }
                        }
                        Thread.Sleep(30);
                        break;
                    case STATE_VAL.CAM_SAVE:
                        if ((m_bCam2NewFrame) && (m_nSaveIndex2 > 1))
                        {
                            m_bCam2NewFrame = false;
                            Buffer.BlockCopy(m_pSaveBuffer2[m_nSaveIndex2 - 1], 0, m_pCam2DisplayBuffer, 0, (int)m_stCam2ImageInfo.nFrameLen);
                            ImageDisplayCam2();
                            Thread.Sleep(30);
                        }
                        break;
                    case STATE_VAL.CAM_SAVE_DONE:
                        if ((m_Cam1State == STATE_VAL.CAM_IDLE) || (m_Cam1State == STATE_VAL.CAM_SAVE_DONE))
                        {
                            GrabInfoTimer.Stop();

                            if (m_MyCamera1 != null)
                            {
                                m_MyCamera1.StopGrabbing();
                                m_Cam1State = STATE_VAL.CAM_OPENED;
                            }

                            if (m_MyCamera2 != null)
                            {
                                m_MyCamera2.StopGrabbing();
                                m_Cam2State = STATE_VAL.CAM_OPENED;
                            }
                        }

                        break;
                    case STATE_VAL.CAM_PLAY:
                        if ((m_pOriginalImage2 == null) && (m_pDisplayImage2 == null))
                        {
                            CreateImageBuffer2();
                        }
                        else
                        {
                            if (m_LastDisplayIndex2 != m_nSaveIndex2)
                            {
                                Buffer.BlockCopy(m_pSaveBuffer2[m_nSaveIndex2], 0, m_pCam2DisplayBuffer, 0, (int)m_stCam2ImageInfo.nFrameLen);
                                ImageDisplayCam2();
                                m_LastDisplayIndex2 = m_nSaveIndex2;
                            }
                        }

                        m_nSaveIndex2++;
                        if (m_nSaveIndex2 == m_Cam2SaveCount)
                        {
                            m_nSaveIndex2--;
                            Thread.Sleep(100);
                            if (m_MyCamera2 != null)
                            {
                                m_Cam2State = STATE_VAL.CAM_OPENED;
                            }
                            else
                            {
                                m_Cam2State = STATE_VAL.CAM_IDLE;
                            }
                                                        
                            m_nSaveIndex2 = 0;
                            Thread.Sleep(300);
                            PlayProgressTimer.Stop();
                            lbState.Invoke(new Action(() =>
                            {
                                lbState.Text = "Camera State : Play End";
                            }));                            
                        }
                        Thread.Sleep(m_nCam2Interval);

                        break;
                    case STATE_VAL.CAM_PLAY_PAUSE:
                        Buffer.BlockCopy(m_pSaveBuffer2[m_nSaveIndex2], 0, m_pCam2DisplayBuffer, 0, (int)m_stCam2ImageInfo.nFrameLen);
                        ImageDisplayCam2();

                        if (m_MyCamera2 != null)
                        {
                            m_Cam2State = STATE_VAL.CAM_OPENED;
                        }
                        else
                        {
                            m_Cam2State = STATE_VAL.CAM_IDLE;
                        }

                        break;
                    default:
                        Thread.Sleep(10);
                        break;
                }
                Thread.Sleep(1);
            }
        }

        private void MemoryInitialize()
        {
            int Cam1DisplayBufferSize = Constants.CAM1_WIDTH * Constants.CAM1_HEIGHT + Constants.MEM_BUFFER_MARGIN;
            int Cam1SaveBufferSize = Constants.CAM1_WIDTH * Constants.CAM1_HEIGHT + Constants.MEM_BUFFER_MARGIN;

            int Cam2DisplayBufferSize = Constants.CAM2_WIDTH * Constants.CAM2_HEIGHT + Constants.MEM_BUFFER_MARGIN;
            int Cam2SaveBufferSize = Constants.CAM2_WIDTH * Constants.CAM2_HEIGHT + Constants.MEM_BUFFER_MARGIN;

            // display Buffer
            m_pCam1DisplayBuffer = new byte[Cam1DisplayBufferSize];
            {
                if (m_pCam1DisplayBuffer == null)
                {
                    return;
                }
            }

            m_pCam2DisplayBuffer = new byte[Cam2DisplayBufferSize];
            {
                if (m_pCam2DisplayBuffer == null)
                {
                    return;
                }
            }

            for (int i = 0; i < Constants.CAM1_SAVE_COUNT; i++)
            {
                m_pSaveBuffer1[i] = new byte[Cam1SaveBufferSize];
            }

            for (int i = 0; i < Constants.CAM2_SAVE_COUNT; i++)
            {
                m_pSaveBuffer2[i] = new byte[Cam2SaveBufferSize];
            }
        }

        private void CreateImageBuffer1()
        {
            int width;
            int height;

            width = m_stCam1ImageInfo.nWidth;
            height = m_stCam1ImageInfo.nHeight;

            m_pOriginalImage1 = new Mat(height, width, MatType.CV_8UC1);
            m_pDisplayImage1 = new Mat(height, width, MatType.CV_8UC3);
            m_pDisplayMode1 = new Mat(height, width, MatType.CV_8UC3);
        }

        private void CreateImageBuffer2()
        {
            int width;
            int height;

            width = m_stCam2ImageInfo.nWidth;
            height = m_stCam2ImageInfo.nHeight;

            m_pOriginalImage2 = new Mat(height, width, MatType.CV_8UC1);
            m_pDisplayImage2 = new Mat(height, width, MatType.CV_8UC3);
            m_pDisplayMode2 = new Mat(height, width, MatType.CV_8UC3);
        }

        private void ImageDisplayCam1()
        {
            m_pOriginalImage1.SetArray(m_pCam1DisplayBuffer);
            Cv2.CvtColor(m_pOriginalImage1, m_pDisplayImage1, ColorConversionCodes.BayerGR2BGR);

            if (m_bFocusMode == true)
            {
                FocusTest(m_pDisplayImage1, 0);
            }

            Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(m_pDisplayImage1);
            pictureBox1.Image = bitmap;
        }


        private void ImageDisplayCam2()
        {
            m_pOriginalImage2.SetArray(m_pCam2DisplayBuffer);
            Cv2.CvtColor(m_pOriginalImage2, m_pDisplayImage2, ColorConversionCodes.BayerGR2BGR);
            if (m_bFocusMode == true)
            {
                FocusTest(m_pDisplayImage2, 1);
            }

            Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(m_pDisplayImage2);
            pictureBox2.Image = bitmap;
        }

        private void ReleaseImageBuffer()
        {
            if (m_pOriginalImage1 != null)
            {
                m_pOriginalImage1.Dispose();
                m_pOriginalImage1 = null;
            }

            if (m_pDisplayImage1 != null)
            {
                m_pDisplayImage1.Dispose();
                m_pDisplayImage1 = null;
            }

            if (m_pDisplayMode1 != null)
            {
                m_pDisplayMode1.Dispose();
                m_pDisplayMode1 = null;
            }

            if (m_pOriginalImage2 != null)
            {
                m_pOriginalImage2.Dispose();
                m_pOriginalImage2 = null;
            }

            if (m_pDisplayImage2 != null)
            {
                m_pDisplayImage2.Dispose();
                m_pDisplayImage2 = null;
            }

            if (m_pDisplayMode2 != null)
            {
                m_pDisplayMode2.Dispose();
                m_pDisplayMode2 = null;
            }
        }


        private void btLiveView_Click(object sender, EventArgs e)
        {
            int nRet;

            if ((m_Cam1State == STATE_VAL.CAM_OPENED) || (m_Cam2State == STATE_VAL.CAM_OPENED))
            {
                ReleaseImageBuffer();
                tb1.Value = 0;
                tb2.Value = 0;

                // 타이머 생성 및 설정
                GrabInfoTimer = new System.Windows.Forms.Timer();
                GrabInfoTimer.Interval = 2000;
                GrabInfoTimer.Tick += Timer_Tick;
                m_nFrameCount1 = 0;
                m_nFrameCount2 = 0;

                //// 타이머 시작
                GrabInfoTimer.Start();
                FPSWatch.Reset();
                FPSWatch.Start();
                if (m_MyCamera1 != null)
                {
                    nRet = m_MyCamera1.StartGrabbing();
                    if (CErrorDefine.MV_OK != nRet)
                    {
                        return;
                    }
                    m_Cam1State = STATE_VAL.CAM_PREVIEW;
                    lbState.Text = "Camera State : " + "Live View";
                }

                if (m_MyCamera2 != null)
                {
                    nRet = m_MyCamera2.StartGrabbing();
                    if (CErrorDefine.MV_OK != nRet)
                    {
                        return;
                    }
                    m_Cam2State = STATE_VAL.CAM_PREVIEW;
                    lbState.Text = "Camera State : " + "Live View";
                }

                if((m_Cam1State == STATE_VAL.CAM_PREVIEW)|| (m_Cam2State == STATE_VAL.CAM_PREVIEW))
                {
                    cbFocusMode.Enabled = true;
                }                
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (m_MyCamera1 != null)
            {
                long elapsedSeconds = (long)FPSWatch.Elapsed.TotalMilliseconds;
                m_nFrameRate1 = (double)(m_nFrameCount1) * 1000 / elapsedSeconds;
                string result = string.Format("Cam1 FPS : {0:F1}", m_nFrameRate1);
                lbCam1FPS.Text = result;
                m_nFrameRate2 = (double)(m_nFrameCount2) * 1000 / elapsedSeconds;
                result = string.Format("Cam2 FPS : {0:F1}", m_nFrameRate2);
                lbCam2FPS.Text = result;
                FPSWatch.Reset();
                m_nFrameCount1 = 0;
                m_nFrameCount2 = 0;
                FPSWatch.Start();
            }
        }

        private void Timer_Tick1(object sender, EventArgs e)
        {
            if ((m_Cam1State == STATE_VAL.CAM_PLAY) && (m_Cam2State == STATE_VAL.CAM_PLAY))
            {
                tb1.Value = m_nSaveIndex1;
                tb2.Value = m_nSaveIndex2;
            }
            else if (m_Cam1State == STATE_VAL.CAM_PLAY)
            {
                if (m_nSaveIndex1 < m_Cam1SaveCount)
                {
                    tb1.Value = m_nSaveIndex1;
                }
            }
            else if (m_Cam2State == STATE_VAL.CAM_PLAY)
            {
                if (m_nSaveIndex2 < m_Cam2SaveCount)
                {
                    tb2.Value = m_nSaveIndex2;
                }
            }
        }

        private void btStop_Click(object sender, EventArgs e)
        {
            int nRet;

            if (((STATE_VAL.CAM_PREVIEW != m_Cam1State) && (STATE_VAL.CAM_PREVIEW != m_Cam2State) && (STATE_VAL.CAM_SAVE_DONE != m_Cam1State) && (STATE_VAL.CAM_SAVE_DONE != m_Cam2State)))
            {
                return;
            }

            GrabInfoTimer.Stop();

            if (m_MyCamera1 != null)
            {
                nRet = m_MyCamera1.StopGrabbing();
                if (CErrorDefine.MV_OK != nRet)
                {
                    return;
                }
                m_Cam1State = STATE_VAL.CAM_OPENED;
                lbState.Text = "Camera State : Camera Ready";
            }

            if (m_MyCamera2 != null)
            {
                nRet = m_MyCamera2.StopGrabbing();
                if (CErrorDefine.MV_OK != nRet)
                {
                    return;
                }
                m_Cam2State = STATE_VAL.CAM_OPENED;
                lbState.Text = "Camera State : Camera Ready";
            }

            if ((m_Cam1State != STATE_VAL.CAM_PREVIEW) && (m_Cam2State != STATE_VAL.CAM_PREVIEW))
            {
                cbFocusMode.Enabled = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (((STATE_VAL.CAM_PREVIEW == m_Cam1State) || (STATE_VAL.CAM_PREVIEW == m_Cam2State) || (STATE_VAL.CAM_SAVE_DONE == m_Cam1State) || (STATE_VAL.CAM_SAVE_DONE == m_Cam2State)))
            {
                btStop_Click(null, EventArgs.Empty);
            }

            if (m_bCam1Thread == true)
            {
                m_bCam1Thread = false;
                m_hCam1ReceiveThread.Join();
            }

            if (m_bCam2Thread == true)
            {
                m_bCam2Thread = false;
                m_hCam2ReceiveThread.Join();
            }

            if (m_MyCamera1 != null)
            {
                m_MyCamera1.CloseDevice();
                m_MyCamera1.DestroyHandle();
            }

            if (m_MyCamera2 != null)
            {
                m_MyCamera2.CloseDevice();
                m_MyCamera2.DestroyHandle();
            }

            ReleaseImageBuffer();
        }

        private void btRecord_Click(object sender, EventArgs e)
        {
            if ((m_Cam1State != STATE_VAL.CAM_PREVIEW) && (m_Cam2State != STATE_VAL.CAM_PREVIEW))
            {
                MessageBox.Show("Recording is possible when in Live View Mode.");
                return;
            }

            m_bCaptureFlag1 = false;
            m_bCaptureFlag2 = false;
            m_nSaveIndex1 = 0;
            m_nSaveIndex2 = 0;
            lbState.Text = "Camstate : Recording 3 sec";

            cbFocusMode.Enabled = false;
            
            if ((m_MyCamera1 != null) && (m_MyCamera2 != null))
            {
                m_Cam1State = STATE_VAL.CAM_SAVE;
                m_Cam2State = STATE_VAL.CAM_SAVE;
            }
            else if (m_MyCamera1 != null)
            {
                m_Cam1State = STATE_VAL.CAM_SAVE;
            }
            else if (m_MyCamera2 != null)
            {
                m_Cam2State = STATE_VAL.CAM_SAVE;
            }
        }

        private void btPlay_Click(object sender, EventArgs e)
        {
            if ((!m_bCaptureFlag1) && (!m_bCaptureFlag2))
            {
                MessageBox.Show("There is nothing to play.");
                return;
            }

            if ((m_Cam1State == STATE_VAL.CAM_PREVIEW) || (m_Cam2State == STATE_VAL.CAM_PREVIEW))
            {
                btStop_Click(null, EventArgs.Empty);
            }

            cbFocusMode.Enabled = false;
            
            m_LastDisplayIndex1 = 0;
            m_LastDisplayIndex2 = 0;

            m_nSaveIndex1 = 0;
            m_nSaveIndex2 = 0;

            tb1.Minimum = 0;
            tb2.Minimum = 0;


            PlayProgressTimer = new System.Windows.Forms.Timer();
            PlayProgressTimer.Interval = 150;
            PlayProgressTimer.Tick += Timer_Tick1;

            // 타이머 시작
            PlayProgressTimer.Start();

            if ((m_bCaptureFlag1) && (m_bCaptureFlag2))
            {
                m_Cam1State = STATE_VAL.CAM_PLAY;
                m_Cam2State = STATE_VAL.CAM_PLAY;
            }
            else if (m_bCaptureFlag1)
            {
                m_Cam1State = STATE_VAL.CAM_PLAY;
            }
            else if (m_bCaptureFlag2)
            {
                m_Cam2State = STATE_VAL.CAM_PLAY;
            }
            lbState.Text = "Camera State : Play";
        }

        private void btCamSetSave_Click(object sender, EventArgs e)
        {
            if ((m_Cam1State == STATE_VAL.CAM_PREVIEW) || (m_Cam2State == STATE_VAL.CAM_PREVIEW))
            {
                btStop_Click(null, EventArgs.Empty);
                Thread.Sleep(100);
            }

            lbState.Text = "Camera State : Save Start";

            if (m_MyCamera1 != null)
            {
                m_MyCamera1.SetStringValue("DeviceUserID", "FON\0");
                Thread.Sleep(100);
                m_MyCamera1.SetEnumValue("UserSetSelector", 1); // userset 1
                Thread.Sleep(300);
                m_MyCamera1.SetEnumValue("UserSetDefault", 1); // userset 1
                Thread.Sleep(300);
                m_MyCamera1.SetCommandValue("UserSetSave"); // userset 1
                Thread.Sleep(2500);
            }

            if (m_MyCamera2 != null)
            {
                m_MyCamera2.SetStringValue("DeviceUserID", "DTL\0");
                Thread.Sleep(100);
                m_MyCamera2.SetEnumValue("UserSetSelector", 1); // userset 1
                Thread.Sleep(300);
                m_MyCamera2.SetEnumValue("UserSetDefault", 1); // userset 1
                Thread.Sleep(300);
                m_MyCamera2.SetCommandValue("UserSetSave"); // userset 1
                Thread.Sleep(2500);
            }

            lbState.Text = "Camera State : Save Done";
        }

        private void InitializeContents()
        {
            tb1.Minimum = 0;
            tb1.Maximum = m_Cam1SaveCount - 1;
            tb1.Value = 0;

            tb2.Minimum = 0;
            tb2.Maximum = m_Cam1SaveCount - 1;
            tb2.Value = 0;

            tbFPS.Minimum = 0;
            tbFPS.Maximum = 3;
            tbFPS.Value = 3;

            tbISO.Minimum = 0;
            tbISO.Maximum = 8;
            tbISO.Value = 4;

            tbExposure.Minimum = 0;
            tbExposure.Maximum = 6;
            tbExposure.Value = 1;

            cbFocusMode.Checked = m_bFocusMode;
            cbFocusMode.Enabled = false;

            GetFrameRate();
            GetGain();
            GetExposureTime();


        }

        private void GetFrameRate()
        {
            int nRet;
            float fps = 120.0f;
            if (m_MyCamera1 != null)
            {
                CFloatValue pcFloatValue = new CFloatValue();
                nRet = m_MyCamera1.GetFloatValue("ResultingFrameRate", ref pcFloatValue);
                if (CErrorDefine.MV_OK == nRet)
                {
                    fps = pcFloatValue.CurValue;
                }

                if (fps < 31)
                {
                    tbFPS.Value = 0;
                    lbFPS.Text = "Frame Rate 30";
                    m_Cam1SaveCount = 30 * 3;
                    m_Cam2SaveCount = 30 * 3;
                    tb1.Maximum = m_Cam1SaveCount - 1;
                    tb2.Maximum = m_Cam1SaveCount - 1;
                    m_nCam1Interval = 25;
                    m_nCam2Interval = 25;
                    m_nSaveFrameRate = 30;
                }
                else if (fps < 61)
                {
                    tbFPS.Value = 1;
                    lbFPS.Text = "Frame Rate 60";
                    m_Cam1SaveCount = 60 * 3;
                    m_Cam2SaveCount = 60 * 3;
                    tb1.Maximum = m_Cam1SaveCount - 1;
                    tb2.Maximum = m_Cam1SaveCount - 1;
                    m_nCam1Interval = 10;
                    m_nCam2Interval = 10;
                    m_nSaveFrameRate = 60;
                }
                else if (fps < 121)
                {
                    tbFPS.Value = 2;
                    lbFPS.Text = "Frame Rate 120";
                    m_Cam1SaveCount = 120 * 3;
                    m_Cam2SaveCount = 120 * 3;
                    tb1.Maximum = m_Cam1SaveCount - 1;
                    tb2.Maximum = m_Cam1SaveCount - 1;
                    m_nCam1Interval = 5;
                    m_nCam2Interval = 5;
                    m_nSaveFrameRate = 120;
                }
                else
                {
                    tbFPS.Value = 3;
                    lbFPS.Text = "Frame Rate 180";
                    m_Cam1SaveCount = 180 * 3;
                    tb1.Maximum = m_Cam1SaveCount - 1;
                    tb2.Maximum = m_Cam1SaveCount - 1;
                    m_nCam1Interval = 2;
                    m_nCam2Interval = 2;
                    m_nSaveFrameRate = 180;
                    bool bValue = true;
                    fps = 180.0f;
                    nRet = m_MyCamera1.SetBoolValue("AcquisitionFrameRateEnable", bValue);
                    nRet = m_MyCamera1.SetFloatValue("AcquisitionFrameRate", fps);
                    if (m_MyCamera2 != null)
                    {
                        nRet = m_MyCamera2.SetBoolValue("AcquisitionFrameRateEnable", bValue);
                        nRet = m_MyCamera2.SetFloatValue("AcquisitionFrameRate", fps);
                    }
                    m_nSaveFrameRate = 180;
                }
            }
        }

        private void SetFrameRate()
        {
            int nRet;
            float fps = 120.0f;
            if (m_MyCamera1 != null)
            {
                if (tbFPS.Value == 0)
                {
                    fps = 30.0f;
                    m_Cam1SaveCount = 30 * 3;
                    tb1.Maximum = m_Cam1SaveCount - 1;
                    m_Cam2SaveCount = 30 * 3;
                    tb2.Maximum = m_Cam2SaveCount - 1;
                    lbFPS.Text = "Frame Rate 30";
                    m_nCam1Interval = 25;
                    m_nCam2Interval = 25;
                    m_nSaveFrameRate = 30;
                }
                else if (tbFPS.Value == 1)
                {
                    fps = 60.0f;
                    m_Cam1SaveCount = 60 * 3;
                    tb1.Maximum = m_Cam1SaveCount - 1;
                    m_Cam2SaveCount = 60 * 3;
                    tb2.Maximum = m_Cam2SaveCount - 1;
                    lbFPS.Text = "Frame Rate 60";
                    m_nCam1Interval = 10;
                    m_nCam2Interval = 10;
                    m_nSaveFrameRate = 60;
                }
                else if (tbFPS.Value == 2)
                {
                    fps = 120.0f;
                    m_Cam1SaveCount = 120 * 3;
                    tb1.Maximum = m_Cam1SaveCount - 1;
                    m_Cam2SaveCount = 120 * 3;
                    tb2.Maximum = m_Cam2SaveCount - 1;
                    lbFPS.Text = "Frame Rate 120";
                    m_nCam1Interval = 5;
                    m_nCam2Interval = 5;
                    m_nSaveFrameRate = 120;
                }
                else
                {
                    fps = 180.0f;
                    m_Cam1SaveCount = 180 * 3;
                    tb1.Maximum = m_Cam1SaveCount - 1;
                    m_Cam2SaveCount = 180 * 3;
                    tb2.Maximum = m_Cam2SaveCount - 1;
                    lbFPS.Text = "Frame Rate 180";
                    m_nCam1Interval = 2;
                    m_nCam2Interval = 2;
                    m_nSaveFrameRate = 180;
                }

                bool bValue = true;

                nRet = m_MyCamera1.SetBoolValue("AcquisitionFrameRateEnable", bValue);
                nRet = m_MyCamera1.SetFloatValue("AcquisitionFrameRate", fps);

                if (m_MyCamera1 != null)
                {
                    nRet = m_MyCamera2.SetBoolValue("AcquisitionFrameRateEnable", bValue);
                    nRet = m_MyCamera2.SetFloatValue("AcquisitionFrameRate", fps);
                }
            }
        }

        private void tbFPS_Scroll(object sender, EventArgs e)
        {
            SetFrameRate();
        }

        private void GetGain()
        {
            int nRet;
            float Gain = 1.0f;
            if (m_MyCamera1 != null)
            {
                CFloatValue pcFloatValue = new CFloatValue();

                nRet = m_MyCamera1.GetFloatValue("Gain", ref pcFloatValue);
                if (CErrorDefine.MV_OK == nRet)
                {
                    Gain = pcFloatValue.CurValue;
                }

                if (Gain < 0.1)
                {
                    tbISO.Value = 0;
                    lbISO.Text = "ISO 50";
                }
                else if (Gain < 10.1)
                {
                    tbISO.Value = 1;
                    lbISO.Text = "ISO 100";
                }
                else if (Gain < 15.1)
                {
                    tbISO.Value = 2;
                    lbISO.Text = "ISO 200";
                }
                else if (Gain < 20.1)
                {
                    tbISO.Value = 3;
                    lbISO.Text = "ISO 400";
                }
                else if (Gain < 25.1)
                {
                    tbISO.Value = 4;
                    lbISO.Text = "ISO 800";
                }
                else if (Gain < 30.1)
                {
                    tbISO.Value = 5;
                    lbISO.Text = "ISO 1600";
                }
                else if (Gain < 35.1)
                {
                    tbISO.Value = 6;
                    lbISO.Text = "ISO 3200";
                }
                else if (Gain < 40.1)
                {
                    tbISO.Value = 7;
                    lbISO.Text = "ISO 6400";
                }
                else
                {
                    tbISO.Value = 8;
                    lbISO.Text = "ISO 12800";
                }
            }
        }

        private void SetGain()
        {
            int nRet;
            float Gain = 20.0f;
            if (m_MyCamera1 != null)
            {
                if (tbISO.Value == 0)
                {
                    Gain = 0.0f;
                    lbISO.Text = "ISO 50";
                }
                else if (tbISO.Value == 1)
                {
                    Gain = 10.0f;
                    lbISO.Text = "ISO 100";
                }
                else if (tbISO.Value == 2)
                {
                    Gain = 15.0f;
                    lbISO.Text = "ISO 200";
                }
                else if (tbISO.Value == 3)
                {
                    Gain = 20.0f;
                    lbISO.Text = "ISO 400";
                }
                else if (tbISO.Value == 4)
                {
                    Gain = 25.0f;
                    lbISO.Text = "ISO 800";
                }
                else if (tbISO.Value == 5)
                {
                    Gain = 30.0f;
                    lbISO.Text = "ISO 1600";
                }
                else if (tbISO.Value == 6)
                {
                    Gain = 35.0f;
                    lbISO.Text = "ISO 3200";
                }
                else if (tbISO.Value == 7)
                {
                    Gain = 40.0f;
                    lbISO.Text = "ISO 6400";
                }
                else
                {
                    Gain = 45.0f;
                    lbISO.Text = "ISO 12800";
                }

                m_MyCamera1.SetEnumValue("GainAuto", 0);
                nRet = m_MyCamera1.SetFloatValue("Gain", Gain);
                if (m_MyCamera2 != null)
                {
                    m_MyCamera2.SetEnumValue("GainAuto", 0);
                    nRet = m_MyCamera2.SetFloatValue("Gain", Gain);
                }
            }
        }

        private void tbISO_Scroll(object sender, EventArgs e)
        {
            SetGain();
        }

        private void GetExposureTime()
        {
            int nRet;
            float ExposureTime = 100.0f;
            if (m_MyCamera1 != null)
            {
                CFloatValue pcFloatValue = new CFloatValue();

                nRet = m_MyCamera1.GetFloatValue("ExposureTime", ref pcFloatValue);
                if (CErrorDefine.MV_OK == nRet)
                {
                    ExposureTime = pcFloatValue.CurValue;
                }

                if (ExposureTime < 101.0f)
                {
                    tbExposure.Value = 0;
                    lbExposure.Text = "Exposure Time 100us";
                }
                else if (ExposureTime < 501.0f)
                {
                    tbExposure.Value = 1;
                    lbExposure.Text = "Exposure Time 500us";
                }
                else if (ExposureTime < 1001.0f)
                {
                    tbExposure.Value = 2;
                    lbExposure.Text = "Exposure Time 1000us";
                }
                else if (ExposureTime < 2001.0f)
                {
                    tbExposure.Value = 3;
                    lbExposure.Text = "Exposure Time 2000us";
                }
                else if (ExposureTime < 3001.0f)
                {
                    tbExposure.Value = 4;
                    lbExposure.Text = "Exposure Time 3000us";
                }
                else if (ExposureTime < 4001.0f)
                {
                    tbExposure.Value = 5;
                    lbExposure.Text = "Exposure Time 4000us";
                }
                else
                {
                    tbExposure.Value = 6;
                    lbExposure.Text = "Exposure Time 5000us";
                }
            }
        }

        private void SetExposureTime()
        {
            int nRet;
            if (m_MyCamera1 != null)
            {
                float ExposureTime = 500.0f;
                if (tbExposure.Value == 0)
                {
                    ExposureTime = 100.0f;
                    lbExposure.Text = "Exposure Time 100us";
                }
                else if (tbExposure.Value == 1)
                {
                    ExposureTime = 500.0f;
                    lbExposure.Text = "Exposure Time 500us";
                }
                else if (tbExposure.Value == 2)
                {
                    ExposureTime = 1000.0f;
                    lbExposure.Text = "Exposure Time 1000us";
                }
                else if (tbExposure.Value == 3)
                {
                    ExposureTime = 2000.0f;
                    lbExposure.Text = "Exposure Time 2000us";
                }
                else if (tbExposure.Value == 4)
                {
                    ExposureTime = 3000.0f;
                    lbExposure.Text = "Exposure Time 3000us";
                }
                else if (tbExposure.Value == 5)
                {
                    ExposureTime = 4000.0f;
                    lbExposure.Text = "Exposure Time 4000us";
                }
                else
                {
                    ExposureTime = 5000.0f;
                    lbExposure.Text = "Exposure Time 5000us";
                }

                nRet = m_MyCamera1.SetFloatValue("ExposureTime", ExposureTime);
                if(m_MyCamera2 != null)
                {
                    nRet = m_MyCamera2.SetFloatValue("ExposureTime", ExposureTime);
                }
            }
        }

        private void tbExposure_Scroll(object sender, EventArgs e)
        {
            SetExposureTime();
        }

        private void tb1_Scroll(object sender, EventArgs e)
        {
            if (m_Cam1State == STATE_VAL.CAM_PLAY) 
            {
                PlayProgressTimer.Stop();
                m_Cam1State = STATE_VAL.CAM_OPENED;
                m_Cam2State = STATE_VAL.CAM_OPENED;
                lbState.Text = "Camera State : Play Pause";
            }
            else if (((m_Cam1State == STATE_VAL.CAM_OPENED) || (m_Cam1State == STATE_VAL.CAM_IDLE)) && m_bCaptureFlag1)
            {
                m_nSaveIndex1 = tb1.Value;
                m_Cam1State = STATE_VAL.CAM_PLAY_PAUSE;
                m_Cam2State = STATE_VAL.CAM_PLAY_PAUSE;
                lbState.Text = "Camera State : Play Pause";
            }
        }

        private void tb2_Scroll(object sender, EventArgs e)
        {
            if (m_Cam2State == STATE_VAL.CAM_PLAY)
            {
                PlayProgressTimer.Stop();
                m_Cam1State = STATE_VAL.CAM_OPENED;
                m_Cam2State = STATE_VAL.CAM_OPENED;
                lbState.Text = "Camera State : Play Pause";
            }
            else if (((m_Cam2State == STATE_VAL.CAM_OPENED) || (m_Cam2State == STATE_VAL.CAM_IDLE)) && m_bCaptureFlag2)
            {
                m_nSaveIndex2 = tb2.Value;
                m_Cam1State = STATE_VAL.CAM_PLAY_PAUSE;
                m_Cam2State = STATE_VAL.CAM_PLAY_PAUSE;
                lbState.Text = "Camera State : Play Pause";
            }
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if ((!m_bCaptureFlag1) && (!m_bCaptureFlag2))
            {
                MessageBox.Show("There is nothing to save.");
                return;
            }

            if ((m_Cam1State == STATE_VAL.CAM_PREVIEW) || (m_Cam2State == STATE_VAL.CAM_PREVIEW))
            {
                btStop_Click(null, EventArgs.Empty);
            }

            cbFocusMode.Enabled = false;
           
            if (saveFileDialog1 == null)
            {
                saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                // 필요에 따라 대화 상자의 초기 설정을 변경할 수 있습니다.
                saveFileDialog1.Filter = "mp4 files (*.mp4)|*.mp4|All files (*.*)|*.*"; // 저장 가능한 파일 형식을 정의
                saveFileDialog1.FilterIndex = 1; // 기본 선택 파일 형식을 정의
                saveFileDialog1.RestoreDirectory = true; // 다음에 대화 상자를 열 때 마지막으로 열렸던 디렉토리를 복원
            }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK) // 사용자가 "Save" 버튼을 클릭하면
            {
                lbState.Text = "Save";

                // 파일 이름과 확장자를 분리
                string fileName = Path.GetFileNameWithoutExtension(saveFileDialog1.FileName);
                string extension = Path.GetExtension(saveFileDialog1.FileName);

                if(m_bCaptureFlag1 == true)
                {
                    // 값 추가
                    string Cam1FileName = fileName + "_cam1" + extension;
                    string newCam1FilePath = Path.Combine(Path.GetDirectoryName(saveFileDialog1.FileName), Cam1FileName);
                    
                    int width = m_stCam1ImageInfo.nWidth;
                    int height = m_stCam1ImageInfo.nHeight;
                    int fps = m_nSaveFrameRate;
                    VideoWriter videoWriter = new VideoWriter(newCam1FilePath, VideoWriter.FourCC('m', 'p', '4', 'v'), fps, // 대소문자 구별해야함!!
                    new OpenCvSharp.Size(width, height), true);

                    if (false == videoWriter.IsOpened())
                    {
                        Console.WriteLine("Can't open VideoWriter");
                        lbState.Text = "Cam1 File save Error";                        
                    }
                    for (int i = 0; i < m_Cam1SaveCount; i++)
                    {
                        m_pOriginalImage1.SetArray(m_pSaveBuffer1[i]);
                        Cv2.CvtColor(m_pOriginalImage1, m_pDisplayImage1, ColorConversionCodes.BayerGR2BGR);
                        videoWriter.Write(m_pDisplayImage1);
                    }

                    videoWriter.Dispose();
                }


                if (m_bCaptureFlag2 == true)
                {
                    // 값 추가
                    string Cam2FileName = fileName + "_cam2" + extension;
                    string newCam2FilePath = Path.Combine(Path.GetDirectoryName(saveFileDialog1.FileName), Cam2FileName);

                    int width = m_stCam2ImageInfo.nWidth;
                    int height = m_stCam2ImageInfo.nHeight;
                    int fps = m_nSaveFrameRate;
                    VideoWriter videoWriter = new VideoWriter(newCam2FilePath, VideoWriter.FourCC('m', 'p', '4', 'v'), fps, // 대소문자 구별해야함!!
                    new OpenCvSharp.Size(width, height), true);

                    if (false == videoWriter.IsOpened())
                    {
                        Console.WriteLine("Can't open VideoWriter");
                        lbState.Text = "Cam2 File save Error";
                        return;
                    }
                    for (int i = 0; i < m_Cam2SaveCount; i++)
                    {
                        m_pOriginalImage2.SetArray(m_pSaveBuffer2[i]);
                        Cv2.CvtColor(m_pOriginalImage2, m_pDisplayImage2, ColorConversionCodes.BayerGR2BGR);
                        videoWriter.Write(m_pDisplayImage2);
                    }

                    videoWriter.Dispose();
                }

                lbState.Text = "Camera State File save complete";
            }
        }


        public int CalculateSumModifiedLaplacian(Mat mat, int nXStart, int nYStart, int nWidth, int nHeight)
        {
            int SML = 0;
            int LocalSML = 0;
            int nChannel = mat.Channels();
            int wBytes = nChannel * nWidth;

            for (int i = nYStart; i < nYStart + nHeight; i++)
            {
                for (int j = nXStart; j < nXStart + nWidth; j++)
                {
                    Vec3b pixel = mat.Get<Vec3b>(i, j);

                    LocalSML = Math.Abs((2 * pixel[0]) - (j > 0 ? mat.Get<Vec3b>(i, j - 1)[0] : pixel[0]) - (j < nWidth - 1 ? mat.Get<Vec3b>(i, j + 1)[0] : pixel[0]))
                               + Math.Abs((2 * pixel[0]) - (i > 0 ? mat.Get<Vec3b>(i - 1, j)[0] : pixel[0]) - (i < nHeight - 1 ? mat.Get<Vec3b>(i + 1, j)[0] : pixel[0]));
                    SML += LocalSML;
                }
            }
            return SML / 1000;
        }


        public void FocusTest(Mat mat, int index)
        {
            int w = mat.Width;
            int h = mat.Height;

            int sx = w / 3;
            int sy = h / 4;
            w = sx;
            h = sy * 2;

            OpenCvSharp.Point pt1 = new OpenCvSharp.Point(sx, sy);
            OpenCvSharp.Point pt2 = new OpenCvSharp.Point(sx + w, sy + h);

            // 이미지 위에 직사각형을 그립니다.
            Cv2.Rectangle(mat, pt1, pt2, Scalar.Red, 1, LineTypes.AntiAlias, 0);

            int CurrentFocus = 0;
            CurrentFocus = CalculateSumModifiedLaplacian(mat, sx, sy, w, h);

            if(index == 0)
            {
                if (CurrentFocus > m_FocusValue1)
                {
                    m_FocusValue1 = CurrentFocus;
                }
            }
            else
            {
                if (CurrentFocus > m_FocusValue2)
                {
                    m_FocusValue2 = CurrentFocus;
                }
            }
            

            OpenCvSharp.Point pt = new OpenCvSharp.Point(sx, sy + h + 50);
            Scalar color = Scalar.Red;
            HersheyFonts fontFace = HersheyFonts.HersheySimplex;
            double fontScale = 1.0;
            int thickness = 2;
            if (index == 0)
            {
                Cv2.PutText(mat, $"Max Focus Value: {m_FocusValue1}", pt, fontFace, fontScale, color, thickness);
            }
            else
            {
                Cv2.PutText(mat, $"Max Focus Value: {m_FocusValue2}", pt, fontFace, fontScale, color, thickness);
            }

            pt = new OpenCvSharp.Point(sx, sy + h + 100);
            color = Scalar.Green;
            Cv2.PutText(mat, $"Current Focus Value: {CurrentFocus}", pt, fontFace, fontScale, color, thickness);

        }

        private void cbFocusMode_CheckedChanged(object sender, EventArgs e)
        {
            m_FocusValue1 = 0;
            m_FocusValue2 = 0;
            m_bFocusMode = cbFocusMode.Checked;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            float width, height;
            int FormWidth;
            int PictureWidth;
            int AvailableWidth;

            FormWidth = this.Width;
            PictureWidth = pictureBox1.Width;

            AvailableWidth = (FormWidth - 244 - PictureWidth * 2 - 50);
            if(AvailableWidth > 200)
            {                
                width = PictureWidth + AvailableWidth / 2;
                height = width * 0.6875f;

                pictureBox1.Width = (int)width;
                pictureBox1.Height = (int)height;
                tb1.Top = 105 + 5 + pictureBox1.Height;
                tb1.Width = (int)width;
                lbCam1FPS.Top = tb1.Top + 60;

                btExchange.Left = pictureBox1.Right - 18;

                pictureBox2.Left = pictureBox1.Right + 30;
                pictureBox2.Width = (int)width;
                pictureBox2.Height = (int)height;
                tb2.Top = tb1.Top;
                tb2.Left = pictureBox2.Left;
                tb2.Width = (int)width;
                lbCam2FPS.Top = lbCam1FPS.Top;
                lbCam2FPS.Left = tb2.Left;
            }           
        }

        private void btExchange_Click(object sender, EventArgs e)
        {
            if(m_nValidCamNum < 2)
            {
                MessageBox.Show("Camera exchange is possible when there are 2 cameras.");
                return;
            }
            // Camera 동작 정지 및 Clear
            if (((STATE_VAL.CAM_PREVIEW == m_Cam1State) || (STATE_VAL.CAM_PREVIEW == m_Cam2State) || (STATE_VAL.CAM_SAVE_DONE == m_Cam1State) || (STATE_VAL.CAM_SAVE_DONE == m_Cam2State)))
            {
                btStop_Click(null, EventArgs.Empty);
            }

            if (m_bCam1Thread == true)
            {
                m_bCam1Thread = false;
                m_hCam1ReceiveThread.Join();
            }

            if (m_bCam2Thread == true)
            {
                m_bCam2Thread = false;
                m_hCam2ReceiveThread.Join();
            }

            if (m_MyCamera1 != null)
            {
                m_MyCamera1.CloseDevice();
                m_MyCamera1.DestroyHandle();
            }

            if (m_MyCamera2 != null)
            {
                m_MyCamera2.CloseDevice();
                m_MyCamera2.DestroyHandle();
            }

            ReleaseImageBuffer();

            int temp;
            temp = m_Cam1Index;
            m_Cam1Index = m_Cam2Index;
            m_Cam2Index =  temp;

            CameraOpen(m_Cam1Index, 0);
            CameraOpen(m_Cam2Index, 1);

            if (m_nValidCamNum > 0)
            {
                ThreadCallbackStart();
                MemoryInitialize();
            }
            InitializeContents();
        }
    }

    public static class Constants
    {
        public const int CAM1_WIDTH = 1600;
        public const int CAM1_HEIGHT = 1100;
        public const int CAM2_WIDTH = 1600;
        public const int CAM2_HEIGHT = 1100;
        public const int MEM_BUFFER_MARGIN = 1024;

        public const int CAM1_SAVE_COUNT = 180 * 3;
        public const int CAM2_SAVE_COUNT = 180 * 3;
        public const double FRAME_RATIO = 1;
        public const int MAX_SAVE_FOLDER = 100;
        public const int CAM1_FPS = 120;
        public const int CAM2_FPS = 120;

        public const int CAM1_DEFAULT_INTERVAL = 1;
        public const int CAM2_DEFAULT_INTERVAL = 1;

    }
}
