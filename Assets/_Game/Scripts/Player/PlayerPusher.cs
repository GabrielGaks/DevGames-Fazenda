using UnityEngine;

namespace DevGames.Player
{
    /// <summary>Permite ao jogador empurrar objetos soltos (com a tag "Empurravel") ao encostar neles.</summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerPusher : MonoBehaviour
    {
        /// <summary>Tag dos objetos que podem ser empurrados.</summary>
        public const string PushableTag = "Empurravel";

        [SerializeField, Min(0f)] float pushForce = 3f;

        /// <summary>Chamado pelo CharacterController quando o jogador colide com algo ao se mover.</summary>
        /// <param name="hit">Dados da colisão: collider atingido, rigidbody e direção do movimento.</param>
        /// <remarks>Só reage à tag "Empurravel"; aplica AddForce na direção horizontal do movimento.</remarks>
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.rigidbody == null || !hit.collider.CompareTag(PushableTag)) return;
            var direction = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
            hit.rigidbody.AddForce(direction * pushForce, ForceMode.Force);
        }
    }
}
