using UnityEngine;  // これが無いと GameObject が解決できません
public static class SelectedPost
{
public static PostData Current;                 // 選択された投稿
public static GameObject LastTappedFirework;    // （任意）元の花火参照
}