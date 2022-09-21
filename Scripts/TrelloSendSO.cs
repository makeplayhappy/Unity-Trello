/*
 * TrelloSend.cs
 * Script that holds keys and allows to send Trello cards.
 *  
 * by Àdam Carballo under MIT license.
 * https://github.com/AdamCarballo/Unity-Trello
 */

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Trello {

    [CreateAssetMenu(fileName = "new TrelloSend", menuName = "ScriptableObjects/TrelloSend", order = 999)]
    public class TrelloSendSO : ScriptableObject {
        

        [Header("Trello Auth")]
        [SerializeField]
        private string _key;
        [SerializeField]
        private string _token;

        [Header("Trello Settings")]
        [SerializeField]
        private string _defaultBoard;
        [SerializeField]
        private string _defaultList;

        public string Key {
            set => _key = value;
        }

        public string Token {
            set => _token = value;
        }
        
        private void Awake() {
            if (string.IsNullOrEmpty(_key) || string.IsNullOrEmpty(_token)) {
                throw new TrelloException("The Trello API key or token are missing!");
            }
        }

        /// <summary>
        /// Sends a given Trello card using the authorization settings must be triggred from a monobehavior (scriptableobjects dont have native Unity Coroutines)
        /// </summary>
        /// <param name="card">Trello card to send.</param>
        /// <param name="list">Overrides default list.</param>
        /// <param name="board">Overrides default board.</param>
        public IEnumerator SendRoutine(TrelloCard card, string list = null, string board = null) {

            if (board == null) {
                board = _defaultBoard;
            }
            if (list == null) {
                list = _defaultList;
            }

            // Create an API instance
            var api = new TrelloAPI(_key, _token);

            // Wait for the Trello boards
            yield return api.PopulateBoards();
            api.SetCurrentBoard(board);

            // Wait for the Trello lists
            yield return api.PopulateLists();
            api.SetCurrentList(list);

            // Set the current ID of the selected list
            card.idList = api.GetCurrentListId();

            UnityWebRequest uwr = api.BuildWebRequest(card);

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success) {
                Debug.Log($"Trello send error: {uwr.error}");
            } else {
                Debug.Log($"Trello card sent!\nResponse {uwr.responseCode}");
            }
	

#if DEBUG_TRELLO			
			string headers = "";
			foreach (KeyValuePair<string, string> kvp in uwr.GetResponseHeaders() ){
				headers += string.Format("Key = {0}, Value = {1} \n", kvp.Key, kvp.Value);
			}
			Debug.Log( headers );
			Debug.Log($"Trello card sent!\nResponse {uwr.responseCode}");
#endif
			
			uwr.Dispose();

        }
    }
}