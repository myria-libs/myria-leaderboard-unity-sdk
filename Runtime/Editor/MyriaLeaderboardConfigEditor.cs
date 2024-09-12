using UnityEditor;
using UnityEngine;
using MyriaLeaderboard;
using MyriaLeaderboard.MyriaLeaderboardExtension;

[CustomEditor(typeof(MyriaLeaderboardConfig))]
public class MyriaLeaderboardConfigEditor : Editor
{
    private string previousDeveloperApiKey;
    private string previousPublicKey;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        // Lấy đối tượng MyriaLeaderboardConfig đang được chỉnh sửa
        MyriaLeaderboardConfig config = (MyriaLeaderboardConfig)target;

        // Hiển thị trường allowCustomUrl trong Inspector
        config.allowCustomUrl = EditorGUILayout.Toggle("Allow Custom URL", config.allowCustomUrl);

        // Kiểm soát trạng thái allowCustomUrl2 dựa trên allowCustomUrl
        EditorGUI.BeginDisabledGroup(!config.allowCustomUrl); // Vô hiệu hóa trường allowCustomUrl2 nếu allowCustomUrl = false
        config.customUrl = EditorGUILayout.TextField("Custom URL", config.customUrl);

        // Lưu thay đổi nếu có sự thay đổi
        if (GUI.changed)
        {
            EditorUtility.SetDirty(config);
        }

        EditorGUI.EndDisabledGroup();
    }
}
