import type { PlanDrillResponse } from '../core/api/models';

export type SectionSaveState = 'idle' | 'saving' | 'error';

export interface EditableSection {
	key: string;
	name: string;
	note: string;
	displayOrder: number;
	planDrills: PlanDrillResponse[];
	saveState: SectionSaveState;
	errorMessage: string | null;
}

