using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardAnimations
{
    public static void FlipAndMoveCard(
        PhysicalCard card,
        Vector3 startPos,
        Vector3 endPos,
        System.Action<PhysicalCard> callback)
    {
        MoveCard(card, startPos, endPos, (movedCard) =>
        {
            FlipCard(movedCard, callback);
        });
    }

    public static void MoveCard(
        PhysicalCard card,
        Vector3 startPos,
        Vector3 endPos,
        System.Action<PhysicalCard> callback = null)
    {
        card.transform.position = startPos;

        card.transform.DOMove(endPos, Globals.CARD_MOVE_TIME).OnComplete(() =>
        {
            callback?.Invoke(card);
        });
    }

    public static void FlipCard(PhysicalCard card, Action<PhysicalCard> callback = null)
    {
        Quaternion startRotation = Quaternion.Euler(0, 0, 180);
        Quaternion endRotation = Quaternion.Euler(0, 0, 0);

        card.transform.DORotateQuaternion(endRotation, Globals.CARD_ANIM_DELAY).OnComplete(() =>
        {
            card.Flipped = true;
            callback?.Invoke(card);
        });
    }

    public static void PopIn(RectTransform transform)
    {
        transform.localScale = new Vector3(0, transform.localScale.y, transform.localScale.z);
        transform.DOScaleX(1, Globals.ROUNDEL_POP_TIME).SetEase(Ease.OutBack);
    }

    public static void BannerPopIn(RectTransform transform, Action callback = null)
    {
        transform.localScale = new Vector3(0, transform.localScale.y, transform.localScale.z);
        transform.DOScaleX(1, Globals.ROUNDEL_POP_TIME).SetEase(Ease.OutBack).OnComplete(() =>
        {
            callback?.Invoke();
        });
    }

    public static void PopOut(RectTransform transform, Action callback = null)
    {
        transform.DOScaleX(0, Globals.ROUNDEL_POP_TIME).SetEase(Ease.InBack).OnComplete(() =>
        {
            callback?.Invoke();
        });
    }
}
