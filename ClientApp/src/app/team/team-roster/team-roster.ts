import { Component, effect, input, output, signal } from '@angular/core';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import type { PlayerResponse } from '../../core/api/models';

type SortColumn = 'lastName' | 'number';
type SortDirection = 'asc' | 'desc';
interface TrackedPlayer {
	_id: number;
	player: PlayerResponse;
}

@Component({
	selector: 'gpp-team-roster',
	templateUrl: './team-roster.html',
	styleUrls: ['./team-roster.css'],
	imports: [...HlmInputImports, ...HlmIconImports],
})
export class TeamRosterComponent {
	readonly players = input.required<PlayerResponse[]>();
	readonly playersChange = output<PlayerResponse[]>();
	private _nextId = 0;
	private _selfEmitted = false;
	readonly trackedPlayers = signal<TrackedPlayer[]>([]);

	readonly sortColumn = signal<SortColumn>('lastName');
	readonly sortDirection = signal<SortDirection>('asc');

	readonly displayPlayers = signal<TrackedPlayer[]>([]);

	constructor() {
		effect(() => {
			const incoming = this.players();
			if (this._selfEmitted) {
				this._selfEmitted = false;
				return;
			}
			this.trackedPlayers.set(
				incoming.map((p) => ({ _id: this._nextId++, player: p }))
			);
			this.applySort();
		});
	}

	sortIcon(column: SortColumn): string {
		if (this.sortColumn() !== column) return 'lucideArrowUpDown';
		return this.sortDirection() === 'asc' ? 'lucideArrowUp' : 'lucideArrowDown';
	}

	toggleSort(column: SortColumn) {
		if (this.sortColumn() === column) {
			this.sortDirection.update((d) => (d === 'asc' ? 'desc' : 'asc'));
		} else {
			this.sortColumn.set(column);
			this.sortDirection.set('asc');
		}
		this.applySort();
	}

	addPlayer() {
		const tracked: TrackedPlayer = {
			_id: this._nextId++,
			player: { lastName: '', number: 0 },
		};
		this.trackedPlayers.update((list) => [...list, tracked]);
		this.displayPlayers.update((list) => [...list, tracked]);
		this.emitPlayers();
	}

	removePlayer(tp: TrackedPlayer) {
		const filter = (list: TrackedPlayer[]) => list.filter((t) => t._id !== tp._id);
		this.trackedPlayers.update(filter);
		this.displayPlayers.update(filter);
		this.emitPlayers();
	}

	updatePlayerLastName(tp: TrackedPlayer, value: string) {
		const update = (list: TrackedPlayer[]) =>
			list.map((t) =>
				t._id === tp._id ? { ...t, player: { ...t.player, lastName: value } } : t
			);
		this.trackedPlayers.update(update);
		this.displayPlayers.update(update);
		this.emitPlayers();
	}

	updatePlayerNumber(tp: TrackedPlayer, value: string) {
		const parsed = parseInt(value, 10);
		const clamped = Math.max(0, Math.min(100, isNaN(parsed) ? 0 : parsed));
		const update = (list: TrackedPlayer[]) =>
			list.map((t) =>
				t._id === tp._id ? { ...t, player: { ...t.player, number: clamped } } : t
			);
		this.trackedPlayers.update(update);
		this.displayPlayers.update(update);
		this.emitPlayers();
	}

	private emitPlayers() {
		this._selfEmitted = true;
		this.playersChange.emit(this.trackedPlayers().map((t) => t.player));
	}

	private applySort() {
		const col = this.sortColumn();
		const dir = this.sortDirection();
		this.displayPlayers.set(
			[...this.trackedPlayers()].sort((a, b) => {
				const cmp =
					col === 'lastName'
						? (a.player.lastName ?? '').localeCompare(b.player.lastName ?? '')
						: (a.player.number ?? 0) - (b.player.number ?? 0);
				return dir === 'asc' ? cmp : -cmp;
			})
		);
	}
}
