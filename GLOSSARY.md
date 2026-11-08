#   Tween glossary

*   **Playable**: a description that can participate in tween playback.
*   **Playback / run**: one activation of a playable, ending in completion or cancellation.
*   **Handle**: control and observation of one playback, rather than every future playback of the same description.
*   **Completion**: the playback reached and applied its intended result.
*   **Kill**: cancellation of a playback without applying its normal completion result.
*   **Progress**: the normalized position along a playback's timeline.
*   **Lane**: the set of playback scheduled for one update phase.
*   **Player**: a reusable presentation command with direction, endpoint and lifecycle behavior.
*   **Player run**: one invocation of a player's playback command, including its repetitions or child playback, ending in completion or cancellation.
*   **Group**: child players played together as one presentation operation.
*   **Sequence**: child players played in order as one presentation operation.
*   **Prepared timeline**: a captured presentation trajectory that gives the same pose when revisiting the same time.
*   **Control**: exclusive ownership of one execution of a prepared timeline, with either an automatic clock or direct time sampling.
