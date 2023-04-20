using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToioAI.CommandInterfaces
{
    public interface ICubeCommand
    {
        void ShowMessage(string message);

        int GetCubePosX(string id);
        int GetCubePosY(string id);

        // https://toio.github.io/toio-spec/docs/ble_motor/#%E6%99%82%E9%96%93%E6%8C%87%E5%AE%9A%E4%BB%98%E3%81%8D%E3%83%A2%E3%83%BC%E3%82%BF%E3%83%BC%E5%88%B6%E5%BE%A1
        // left       | 左モーター速度 | 範囲(0~100)
        // right      | 右モーター速度 | 範囲(0~100)
        // durationMs | 持続時間　　　 | 範囲(0~2550)
        // order      | 優先度　　　　 | 種類(Week, Strong)
        // orderは必要になったら追加する
        IEnumerator Move(string id, int left, int right, int durationMs);

        // Move to target position
        // https://toio.github.io/toio-spec/docs/hardware_position_id/
        // x: 45~455, y: 45~455
        IEnumerator Navi2TargetCoroutine(string id, double x, double y, int rotateTime = 250, float timeout = 5f);

        // deg. world top: 90, right: 0 
        IEnumerator Rotate2DegCoroutine(string id, double deg, int rotateTime = 250, float timeout = 5f);
    }
}
