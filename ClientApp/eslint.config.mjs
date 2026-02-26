import nx from '@nx/eslint-plugin';
import prettier from 'eslint-config-prettier';

export default [
	...nx.configs['flat/base'],
	...nx.configs['flat/typescript'],
	...nx.configs['flat/javascript'],
	{
		ignores: ['**/dist', '**/out-tsc', 'src/app/core/api/**'],
	},
	{
		files: ['**/*.ts', '**/*.tsx', '**/*.cts', '**/*.mts', '**/*.js', '**/*.jsx', '**/*.cjs', '**/*.mjs'],
		rules: {
			'no-console': ['warn', { allow: ['warn', 'error'] }],
			'eqeqeq': ['error', 'always', { null: 'ignore' }],
			'no-debugger': 'error',
		},
	},
	...nx.configs['flat/angular'],
	...nx.configs['flat/angular-template'],
	{
		files: ['**/*.ts'],
		rules: {
			'@angular-eslint/directive-selector': [
				'error',
				{
					type: 'attribute',
					prefix: 'gpp',
					style: 'camelCase',
				},
			],
			'@angular-eslint/component-selector': [
				'error',
				{
					type: 'element',
					prefix: 'gpp',
					style: 'kebab-case',
				},
			],
			'@angular-eslint/use-lifecycle-interface': 'error',
			'@angular-eslint/no-empty-lifecycle-method': 'error',
			'@angular-eslint/prefer-output-readonly': 'error',
			'@typescript-eslint/no-explicit-any': 'warn',
			'@typescript-eslint/consistent-type-imports': [
				'error',
				{ prefer: 'type-imports', fixStyle: 'inline-type-imports' },
			],
		},
	},
	{
		files: ['**/*.html'],
		rules: {
			'@angular-eslint/template/no-negated-async': 'error',
		},
	},
	prettier,
];
