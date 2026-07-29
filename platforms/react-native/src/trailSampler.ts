/**
 * Pure geometry for placing history-trail dots along a recent path. Returns
 * coordinates only — no rendering dependency.
 */

import { bearing, distanceMeters, offset } from './geo.js';
import type { Coordinate } from './types.js';

const M_PER_NM = 1852;

/**
 * `count` positions evenly spread along the recent path (last 8 samples).
 * Ordered oldest → newest.
 */
export function equalSpaced(
  history: readonly Coordinate[],
  count: number
): Coordinate[] {
  const window = history.slice(-8);
  if (window.length < 2) return window.slice(-count);

  const cum = new Array<number>(window.length).fill(0);
  for (let i = 1; i < window.length; i++) {
    cum[i] = cum[i - 1]! + distanceMeters(window[i - 1]!, window[i]!);
  }
  const total = cum[cum.length - 1]!;
  if (total <= 0) return [window[window.length - 1]!];

  const result: Coordinate[] = [];
  for (let k = 1; k <= count; k++) {
    const target = (total * k) / count;
    let seg = window.length - 2;
    for (let i = 0; i < window.length - 1; i++) {
      if (cum[i + 1]! >= target) {
        seg = i;
        break;
      }
    }
    const seg1 = Math.min(seg + 1, window.length - 1);
    const segLen = cum[seg1]! - cum[seg]!;
    const t = segLen > 0 ? (target - cum[seg]!) / segLen : 0;
    result.push({
      latitude:
        window[seg]!.latitude +
        t * (window[seg1]!.latitude - window[seg]!.latitude),
      longitude:
        window[seg]!.longitude +
        t * (window[seg1]!.longitude - window[seg]!.longitude),
    });
  }
  return result;
}

/**
 * Exactly `count` positions spaced `spacingNM` NM apart, walking backward from
 * the newest point; short history is projected backward. Oldest → newest.
 */
export function fixedSpaced(
  history: readonly Coordinate[],
  count: number,
  spacingNM: number
): Coordinate[] {
  if (history.length < 2) return [];
  const spacingMeters = spacingNM * M_PER_NM;

  const result: Coordinate[] = [];
  let walked = 0;
  let dotNum = 1;
  let i = history.length - 1;

  while (i > 0 && result.length < count) {
    const segTo = history[i]!;
    const segFrom = history[i - 1]!;
    const segLen = distanceMeters(segFrom, segTo);

    while (dotNum * spacingMeters <= walked + segLen && result.length < count) {
      const t = segLen > 0 ? (dotNum * spacingMeters - walked) / segLen : 0;
      result.push({
        latitude: segTo.latitude + t * (segFrom.latitude - segTo.latitude),
        longitude: segTo.longitude + t * (segFrom.longitude - segTo.longitude),
      });
      dotNum += 1;
    }

    walked += segLen;
    i -= 1;
  }

  if (result.length < count) {
    const backBearing = bearing(history[1]!, history[0]!);
    while (result.length < count) {
      const extra = dotNum * spacingMeters - walked;
      result.push(offset(history[0]!, extra, backBearing));
      dotNum += 1;
    }
  }

  return result.reverse();
}

/** Namespace object mirroring the Swift `TrailSampler` enum. */
export const TrailSampler = { equalSpaced, fixedSpaced } as const;
