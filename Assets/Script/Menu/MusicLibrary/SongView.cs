using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YARG.Core.Audio;
using YARG.Deluxe;
using YARG.Menu.ListMenu;
using YARG.Settings;

namespace YARG.Menu.MusicLibrary
{
    public class SongView : ViewObject<ViewType>
    {
        [SerializeField]
        private GameObject _songNameContainer;
        [SerializeField]
        private TextMeshProUGUI _sideText;
        [SerializeField]
        private StarView _starView;

        [Space]
        [SerializeField]
        private GameObject _secondaryTextContainer;
        [SerializeField]
        private GameObject _asMadeFamousByTextContainer;

        [Space]
        [SerializeField]
        private GameObject _favoriteButtonContainer;
        [SerializeField]
        private GameObject _favoriteButtonContainerSelected;
        [SerializeField]
        private Image[] _favoriteButtons;

        [Space]
        [SerializeField]
        private Sprite _favoriteUnfilled;
        [SerializeField]
        private Sprite _favouriteFilled;

        [Space]
        [SerializeField]
        private GameObject _categoryNameContainer;
        [SerializeField]
        private TextMeshProUGUI _categoryText;

        [Space]
        [SerializeField] private GameObject _mainContainer;

        [Space]
        public GameObject _offsetContainer;

        Vector2 defSize;
        Vector2 oldSongNameDefPos;
        Vector2 oldCategoryDefPos;

        public static bool IsInCooldown = false;

        private void Start()
        {
            defSize = GetComponent<RectTransform>().sizeDelta;
        }

        public override void Show(bool selected, ViewType viewType, int relativeIndex)
        {
            if (selected)
            {
                GameObject oldContainer = Instantiate(_mainContainer, _offsetContainer.transform);
                RectTransform oldContainerRT = oldContainer.GetComponent<RectTransform>();
                oldContainerRT.DOKill();
                IsInCooldown = true;
                oldContainerRT.DOAnchorPosY(MusicLibraryMenu.Up ? (oldContainerRT.anchoredPosition.y - 110) : (oldContainerRT.anchoredPosition.y + 110), MusicLibraryMenu.IsHolding ? 0.125f : 0.25f).SetEase(Ease.Linear).OnComplete(() => {Destroy(oldContainer); IsInCooldown = false; });

                GameObject oldIcon = Instantiate(_icon.gameObject, _offsetContainer.transform);
                RectTransform oldIconRT = oldIcon.GetComponent<RectTransform>();
                oldIconRT.DOKill();
                oldIconRT.DOAnchorPosY(MusicLibraryMenu.Up ? (oldContainerRT.anchoredPosition.y - 110) : (oldContainerRT.anchoredPosition.y + 110), MusicLibraryMenu.IsHolding ? 0.125f : 0.25f).SetEase(Ease.Linear).OnComplete(() => { Destroy(oldContainer); IsInCooldown = false; });
            }
            base.Show(selected, viewType, relativeIndex);

            if (!selected)
            {
                
                RectTransform rectT = _offsetContainer.GetComponent<RectTransform>();
                Debug.Log(rectT);
                rectT.DOKill();

                /*if(relativeIndex == 1 && !MusicLibraryMenu.Up)
                {
                    rectT.anchoredPosition = new Vector2(0, 0);
                    rectT.DOAnchorPosY(60, 0.25f).SetEase(Ease.Linear);
                }
                else
                {*/
                rectT.anchoredPosition = new Vector2(0, (MusicLibraryMenu.Up ? 60 : -60)) ;
                    rectT.DOAnchorPosY(0, MusicLibraryMenu.IsHolding ? 0.125f:0.25f).SetEase(Ease.Linear);
                //}
                
            }

            // use category header primary text (which supports wider text), when used as section header
            if(viewType.UseWiderPrimaryText)
            {
                _songNameContainer.SetActive(false);
                _categoryNameContainer.SetActive(true);
            }
            else
            {
                _songNameContainer.SetActive(true);
                _categoryNameContainer.SetActive(false);
            }

            // Set side text
            _sideText.text = viewType.GetSideText(selected);

            // Set star view
            var starAmount = viewType.GetStarAmount();
            _starView.gameObject.SetActive(starAmount is not null);
            if (starAmount is not null)
            {
                _starView.SetStars(starAmount.Value);
            }

            // Set "As Made Famous By" text
            _asMadeFamousByTextContainer.SetActive(viewType.UseAsMadeFamousBy);

            // Show/hide favorite button

            var favoriteInfo = viewType.GetFavoriteInfo();

            if (SettingsManager.Settings.ShowFavoriteButton.Value)
            {
                _favoriteButtonContainer.SetActive(!selected && favoriteInfo.ShowFavoriteButton);
                _favoriteButtonContainerSelected.SetActive(selected && favoriteInfo.ShowFavoriteButton);
                UpdateFavoriteSprite(favoriteInfo);
            }
            else
            {
                _favoriteButtonContainer.SetActive(false);
                _favoriteButtonContainerSelected.SetActive(false);
            }

            RectTransform rt = GetComponent<RectTransform>();

            if (selected)
            {
                GetComponent<RectTransform>().sizeDelta = new Vector2(rt.sizeDelta.x, 160);

                

                RectTransform containerRT = _mainContainer.GetComponent<RectTransform>();

                containerRT.DOKill();
                containerRT.anchoredPosition = new Vector2(45, MusicLibraryMenu.Up ? (1.5f + 100) : (1.5f - 100));
                containerRT.DOAnchorPosY(1.5f, MusicLibraryMenu.IsHolding ? 0.125f : 0.25f).SetEase(Ease.Linear);


                RectTransform iconrT = _icon.GetComponent<RectTransform>();

                iconrT.DOKill();
                iconrT.anchoredPosition = new Vector2(55, MusicLibraryMenu.Up ? (1.5f + 100) : (1.5f - 100));
                iconrT.DOAnchorPosY(1.5f, MusicLibraryMenu.IsHolding ? 0.125f : 0.25f).SetEase(Ease.Linear);

                CustomSFX.PlaySoundEffect("uiselect.wav");
            }
            else
            {
                GetComponent<RectTransform>().sizeDelta = new Vector2(rt.sizeDelta.x, 60);
            }
        }

        private void UpdateFavoriteSprite(ViewType.FavoriteInfo favoriteInfo)
        {
            if (!favoriteInfo.ShowFavoriteButton) return;

            foreach (var button in _favoriteButtons)
            {
                button.sprite = favoriteInfo.IsFavorited
                    ? _favouriteFilled
                    : _favoriteUnfilled;
            }
        }

        public void PrimaryTextClick()
        {
            if (!Showing) return;

            ViewType.PrimaryButtonClick();
        }

        public void SecondaryTextClick()
        {
            if (!Showing) return;

            ViewType.SecondaryTextClick();
        }

        public void FavoriteClick()
        {
            if (!Showing) return;

            ViewType.FavoriteClick();

            // Update the sprite after in case the state changed
            UpdateFavoriteSprite(ViewType.GetFavoriteInfo());
        }
    }
}