using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using RenderHeads.Media.AVProMovieCapture;
namespace _KINLAB
{
    public class BodyRecorder : MonoBehaviour
    {
        [SerializeField]
        HumanBodyTracking humanBodyTracking;
        JointIndices3D[] dataindex = {
            JointIndices3D.Head,
            JointIndices3D.Neck1,
            JointIndices3D.LeftArm,
            JointIndices3D.RightArm,
            JointIndices3D.LeftForearm,
            JointIndices3D.RightForearm,
            JointIndices3D.LeftHand,
            JointIndices3D.RightHand,
            JointIndices3D.LeftUpLeg,
            JointIndices3D.RightUpLeg,
            JointIndices3D.LeftLeg,
            JointIndices3D.RightLeg,
            JointIndices3D.LeftFoot,
            JointIndices3D.RightFoot
        };
        string category = "Date,Timestamp,"
                                           + "EventLog,"
                                           + "EventLog_OneShot,";
        [SerializeField]
        CaptureFromScreen capture;
        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < dataindex.Length; i++)
            {
                category += $"{dataindex[i].ToString()}_Pos_x,";
                category += $"{dataindex[i].ToString()}_Pos_y,";
                category += $"{dataindex[i].ToString()}_Pos_z,";

                category += $"{dataindex[i].ToString()}_Rot_x,";
                category += $"{dataindex[i].ToString()}_Rot_y,";
                category += $"{dataindex[i].ToString()}_Rot_z,";
            }
            if (category.Length > 0 && category[category.Length - 1] == ',')
            {
                category.Remove(category.Length - 1, 1);
            }
            GM_DataRecorder.instance.SetFile("DTX", category);
            capture = gameObject.GetComponent<CaptureFromScreen>();

            //capture.OutputFolderPath = GM_DataRecorder.instance.GetFolderPath();
            capture.CompletedFileWritingAction += SaveEnd;
        }

        // Update is called once per frame
        void Update()
        {
            if (GM_DataRecorder.instance.isRecording)
            {
                Record_BoxMovement();
            }
        }
        //bodytracking data 레코더에 삽입 
        public void Record_BoxMovement()
        {
            StringBuilder sb = new StringBuilder();

            //if EventLog added this script will be back
            //sb.Append(",");

            //for(int i=0; i< humanBodyTracking.bodyJoints.Count;i++)
            //{

            //Human Body Record
            #region HumanBody
            if (humanBodyTracking.bodyJoints != null && humanBodyTracking.bodyJoints.Count != 0)
            {
                for (int i = 0; i < dataindex.Length; i++)
                {
                    sb.AppendFormat("{0:F4}", humanBodyTracking.bodyJoints[dataindex[i]].gameObject.transform.position.x).Append(',');
                    sb.AppendFormat("{0:F4}", humanBodyTracking.bodyJoints[dataindex[i]].gameObject.transform.position.y).Append(',');
                    sb.AppendFormat("{0:F4}", humanBodyTracking.bodyJoints[dataindex[i]].gameObject.transform.position.z).Append(',');

                    sb.AppendFormat("{0:F4}", humanBodyTracking.bodyJoints[dataindex[i]].gameObject.transform.rotation.x).Append(',');
                    sb.AppendFormat("{0:F4}", humanBodyTracking.bodyJoints[dataindex[i]].gameObject.transform.rotation.y).Append(',');
                    sb.AppendFormat("{0:F4}", humanBodyTracking.bodyJoints[dataindex[i]].gameObject.transform.rotation.z).Append(',');
                }
            }
            else
            {
                for (int i = 0; i < dataindex.Length; i++)
                {
                    sb.Append("0").Append(',');
                    sb.Append("0").Append(',');
                    sb.Append("0").Append(',');

                    sb.Append("0").Append(',');
                    sb.Append("0").Append(',');
                    sb.Append("0").Append(',');
                }
            }
            #endregion
            if (sb.Length > 0 && sb[sb.Length - 1] == ',')
            {
                sb.Remove(sb.Length - 1, 1);
            }

            _KINLAB.GM_DataRecorder.instance.Enequeue_Data("DTX", sb.ToString());
        }
        public void StartVideo()
        {
            GM_DataRecorder.instance.DataClear("DTX");
            capture.StartCapture();
            GM_DataRecorder.instance.isRecording = true;
        }
        public void SaveVideo()
        {
            Debug.Log("SaveFolderPath" + GM_DataRecorder.instance.GetFolderPath());
            Debug.Log("OutputFolderPath" + capture.OutputFolderPath);
            capture.StopCapture();
            GM_DataRecorder.instance.isRecording = false;
        }
        public void SaveEnd(FileWritingHandler handler)
        {
            Debug.Log("The Last captured file is at:" + capture.LastFilePath);
            GM_DataRecorder.instance_FileSender.StartFileSend(capture.LastFilePath);
          //  Application.Quit();
        }
    }
}