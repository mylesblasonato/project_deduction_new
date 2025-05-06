#if UNITY_EDITOR

using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Slate
{

    [CustomEditor(typeof(ActionClips.CharacterExpression))]
    public class CharacterExpressionInspector : ActionClipInspector<ActionClips.CharacterExpression>
    {

        public override void OnInspectorGUI() {

            base.ShowCommonInspector();

            if ( action.actor != null ) {
                var character = action.actor.GetComponent<Character>();
                if ( character != null ) {
                    BlendShapeGroup current = null;
                    if ( !string.IsNullOrEmpty(action.expressionUID) ) { current = character.FindExpressionByUID(action.expressionUID); } else { current = character.FindExpressionByName(action.expressionName); }
                    var newExp = EditorTools.Popup<BlendShapeGroup>("Expression", current, character.expressions);
                    action.expressionName = newExp != null ? newExp.name : null;
                    action.expressionUID = newExp != null ? newExp.UID : null;
                }
            }

            base.ShowAnimatableParameters();
        }
    }
}

#endif